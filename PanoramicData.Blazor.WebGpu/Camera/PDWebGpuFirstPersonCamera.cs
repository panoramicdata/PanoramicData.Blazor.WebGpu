using System.Numerics;

namespace PanoramicData.Blazor.WebGpu.Camera;

/// <summary>
/// First-person camera with WASD movement and mouse look.
/// Suitable for FPS-style navigation.
/// </summary>
public class PDWebGpuFirstPersonCamera : PDWebGpuCameraBase
{
	private Vector3 _position = new(0, 1.7f, 0); // Default eye height
	private float _yaw; // Rotation around Y axis (in radians)
	private float _pitch; // Rotation around X axis (in radians)
	private float _fieldOfView = MathF.PI / 4f; // 45 degrees
	private float _moveSpeed = 5f; // Units per second
	private float _mouseSensitivity = 0.002f; // Radians per pixel
	private float _minPitch = -MathF.PI / 2f + 0.01f;
	private float _maxPitch = MathF.PI / 2f - 0.01f;

	/// <summary>
	/// Gets or sets the camera position in world space.
	/// </summary>
	public Vector3 Position
	{
		get => _position;
		set
		{
			if (_position != value)
			{
				_position = value;
				MarkViewMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the yaw angle (rotation around Y axis) in radians.
	/// </summary>
	public float Yaw
	{
		get => _yaw;
		set
		{
			if (_yaw != value)
			{
				_yaw = value;
				MarkViewMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the pitch angle (rotation around X axis) in radians.
	/// </summary>
	public float Pitch
	{
		get => _pitch;
		set
		{
			var clampedValue = Math.Clamp(value, _minPitch, _maxPitch);
			if (_pitch != clampedValue)
			{
				_pitch = clampedValue;
				MarkViewMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the field of view angle in radians.
	/// </summary>
	public float FieldOfView
	{
		get => _fieldOfView;
		set
		{
			if (_fieldOfView != value)
			{
				_fieldOfView = value;
				MarkProjectionMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the movement speed in units per second.
	/// </summary>
	public float MoveSpeed
	{
		get => _moveSpeed;
		set => _moveSpeed = value;
	}

	/// <summary>
	/// Gets or sets the mouse sensitivity in radians per pixel.
	/// </summary>
	public float MouseSensitivity
	{
		get => _mouseSensitivity;
		set => _mouseSensitivity = value;
	}

	/// <summary>
	/// Gets the forward direction vector.
	/// </summary>
	public Vector3 Forward
	{
		get
		{
			return new Vector3(
				MathF.Cos(_yaw) * MathF.Cos(_pitch),
				MathF.Sin(_pitch),
				MathF.Sin(_yaw) * MathF.Cos(_pitch)
			);
		}
	}

	/// <summary>
	/// Gets the right direction vector.
	/// </summary>
	public Vector3 Right
	{
		get
		{
			return Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
		}
	}

	/// <summary>
	/// Gets the up direction vector.
	/// </summary>
	public Vector3 Up
	{
		get
		{
			return Vector3.Normalize(Vector3.Cross(Right, Forward));
		}
	}

	/// <summary>
	/// Rotates the camera by the specified mouse delta.
	/// </summary>
	/// <param name="deltaX">Mouse movement in X direction (pixels).</param>
	/// <param name="deltaY">Mouse movement in Y direction (pixels).</param>
	public void Look(float deltaX, float deltaY)
	{
		Yaw += deltaX * _mouseSensitivity;
		Pitch -= deltaY * _mouseSensitivity; // Inverted Y
	}

	/// <summary>
	/// Moves the camera forward/backward.
	/// </summary>
	/// <param name="amount">Movement amount (positive = forward, negative = backward).</param>
	/// <param name="deltaTime">Time delta in seconds.</param>
	public void MoveForward(float amount, float deltaTime)
	{
		var forward = new Vector3(Forward.X, 0, Forward.Z); // Keep on horizontal plane
		forward = Vector3.Normalize(forward);
		Position += forward * amount * _moveSpeed * deltaTime;
	}

	/// <summary>
	/// Moves the camera right/left.
	/// </summary>
	/// <param name="amount">Movement amount (positive = right, negative = left).</param>
	/// <param name="deltaTime">Time delta in seconds.</param>
	public void MoveRight(float amount, float deltaTime)
	{
		Position += Right * amount * _moveSpeed * deltaTime;
	}

	/// <summary>
	/// Moves the camera up/down.
	/// </summary>
	/// <param name="amount">Movement amount (positive = up, negative = down).</param>
	/// <param name="deltaTime">Time delta in seconds.</param>
	public void MoveUp(float amount, float deltaTime)
	{
		Position += Vector3.UnitY * amount * _moveSpeed * deltaTime;
	}

	/// <inheritdoc/>
	protected override Matrix4x4 CalculateViewMatrix()
	{
		var target = _position + Forward;
		return Matrix4x4.CreateLookAt(_position, target, Vector3.UnitY);
	}

	/// <inheritdoc/>
	protected override Matrix4x4 CalculateProjectionMatrix()
	{
		return Matrix4x4.CreatePerspectiveFieldOfView(_fieldOfView, AspectRatio, NearPlane, FarPlane);
	}
}
