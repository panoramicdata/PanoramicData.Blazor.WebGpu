using System.Numerics;

namespace PanoramicData.Blazor.WebGpu.Camera;

/// <summary>
/// Orbit camera that rotates around a target point.
/// Suitable for viewing 3D objects and scenes.
/// </summary>
public class PDWebGpuOrbitCamera : PDWebGpuCameraBase
{
	private Vector3 _target = Vector3.Zero;
	private float _distance = 10f;
	private float _yaw; // Rotation around Y axis (in radians)
	private float _pitch; // Rotation around X axis (in radians)
	private float _fieldOfView = MathF.PI / 4f; // 45 degrees
	private float _minDistance = 1f;
	private float _maxDistance = 100f;
	private float _minPitch = -MathF.PI / 2f + 0.01f;
	private float _maxPitch = MathF.PI / 2f - 0.01f;

	/// <summary>
	/// Gets or sets the target point the camera orbits around.
	/// </summary>
	public Vector3 Target
	{
		get => _target;
		set
		{
			if (_target != value)
			{
				_target = value;
				MarkViewMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the distance from the target.
	/// </summary>
	public float Distance
	{
		get => _distance;
		set
		{
			var clampedValue = Math.Clamp(value, _minDistance, _maxDistance);
			if (_distance != clampedValue)
			{
				_distance = clampedValue;
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
	/// Gets or sets the minimum orbit distance.
	/// </summary>
	public float MinDistance
	{
		get => _minDistance;
		set => _minDistance = value;
	}

	/// <summary>
	/// Gets or sets the maximum orbit distance.
	/// </summary>
	public float MaxDistance
	{
		get => _maxDistance;
		set => _maxDistance = value;
	}

	/// <summary>
	/// Gets the camera position in world space.
	/// </summary>
	public Vector3 Position
	{
		get
		{
			var offset = new Vector3(
				MathF.Cos(_yaw) * MathF.Cos(_pitch),
				MathF.Sin(_pitch),
				MathF.Sin(_yaw) * MathF.Cos(_pitch)
			) * _distance;
			return _target + offset;
		}
	}

	/// <summary>
	/// Rotates the camera by the specified angles.
	/// </summary>
	/// <param name="deltaYaw">Yaw rotation in radians.</param>
	/// <param name="deltaPitch">Pitch rotation in radians.</param>
	public void Rotate(float deltaYaw, float deltaPitch)
	{
		Yaw += deltaYaw;
		Pitch += deltaPitch;
	}

	/// <summary>
	/// Zooms the camera by the specified amount.
	/// </summary>
	/// <param name="deltaDistance">Distance to zoom (negative to zoom in, positive to zoom out).</param>
	public void Zoom(float deltaDistance)
	{
		Distance += deltaDistance;
	}

	/// <inheritdoc/>
	protected override Matrix4x4 CalculateViewMatrix()
	{
		var position = Position;
		var up = Vector3.UnitY;
		return Matrix4x4.CreateLookAt(position, _target, up);
	}

	/// <inheritdoc/>
	protected override Matrix4x4 CalculateProjectionMatrix()
	{
		// WebGPU uses: Y-down NDC, depth [0,1]
		// .NET Matrix4x4 is row-major, so we build it that way
		
		float f = 1.0f / MathF.Tan(_fieldOfView / 2.0f);
		float rangeInv = 1.0f / (NearPlane - FarPlane);

		// Row-major perspective matrix for WebGPU (will be transposed before upload)
		return new Matrix4x4(
			f / AspectRatio, 0, 0, 0,                          // Row 0
			0, f, 0, 0,                                         // Row 1 (positive, not negative!)
			0, 0, FarPlane * rangeInv, NearPlane * FarPlane * rangeInv,  // Row 2 (depth [0,1])
			0, 0, -1, 0                                         // Row 3
		);
	}
}
