using System.Numerics;

namespace PanoramicData.Blazor.WebGpu.Camera;

/// <summary>
/// Orthographic camera with parallel projection.
/// Suitable for 2D rendering, UI, and technical drawings.
/// </summary>
public class PDWebGpuOrthographicCamera : PDWebGpuCameraBase
{
	private Vector3 _position = new(0, 0, 10);
	private Vector3 _target = Vector3.Zero;
	private float _left = -10f;
	private float _right = 10f;
	private float _bottom = -10f;
	private float _top = 10f;
	private float _zoom = 1f;

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
	/// Gets or sets the target point the camera looks at.
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
	/// Gets or sets the left boundary of the view volume.
	/// </summary>
	public float Left
	{
		get => _left;
		set
		{
			if (_left != value)
			{
				_left = value;
				MarkProjectionMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the right boundary of the view volume.
	/// </summary>
	public float Right
	{
		get => _right;
		set
		{
			if (_right != value)
			{
				_right = value;
				MarkProjectionMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the bottom boundary of the view volume.
	/// </summary>
	public float Bottom
	{
		get => _bottom;
		set
		{
			if (_bottom != value)
			{
				_bottom = value;
				MarkProjectionMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the top boundary of the view volume.
	/// </summary>
	public float Top
	{
		get => _top;
		set
		{
			if (_top != value)
			{
				_top = value;
				MarkProjectionMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Gets or sets the zoom level (1.0 = normal, > 1.0 = zoomed in, < 1.0 = zoomed out).
	/// </summary>
	public float Zoom
	{
		get => _zoom;
		set
		{
			if (_zoom != value && value > 0)
			{
				_zoom = value;
				MarkProjectionMatrixDirty();
			}
		}
	}

	/// <summary>
	/// Sets the view bounds based on width and height, centered at origin.
	/// </summary>
	/// <param name="width">Width of the view.</param>
	/// <param name="height">Height of the view.</param>
	public void SetBounds(float width, float height)
	{
		var halfWidth = width / 2f;
		var halfHeight = height / 2f;
		
		Left = -halfWidth;
		Right = halfWidth;
		Bottom = -halfHeight;
		Top = halfHeight;
	}

	/// <summary>
	/// Sets the view bounds to match the aspect ratio while maintaining the specified height.
	/// </summary>
	/// <param name="height">Height of the view.</param>
	public void SetBoundsFromHeight(float height)
	{
		var halfHeight = height / 2f;
		var halfWidth = halfHeight * AspectRatio;
		
		Left = -halfWidth;
		Right = halfWidth;
		Bottom = -halfHeight;
		Top = halfHeight;
	}

	/// <summary>
	/// Pans the camera by the specified amount in screen space.
	/// </summary>
	/// <param name="deltaX">X-axis pan amount.</param>
	/// <param name="deltaY">Y-axis pan amount.</param>
	public void Pan(float deltaX, float deltaY)
	{
		var right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, _position - _target));
		var up = Vector3.Normalize(Vector3.Cross(_position - _target, right));
		
		var offset = right * deltaX + up * deltaY;
		Position += offset;
		Target += offset;
	}

	/// <inheritdoc/>
	protected override Matrix4x4 CalculateViewMatrix()
	{
		return Matrix4x4.CreateLookAt(_position, _target, Vector3.UnitY);
	}

	/// <inheritdoc/>
	protected override Matrix4x4 CalculateProjectionMatrix()
	{
		var zoomedLeft = _left / _zoom;
		var zoomedRight = _right / _zoom;
		var zoomedBottom = _bottom / _zoom;
		var zoomedTop = _top / _zoom;

		return Matrix4x4.CreateOrthographic(
			zoomedRight - zoomedLeft,
			zoomedTop - zoomedBottom,
			NearPlane,
			FarPlane);
	}
}
