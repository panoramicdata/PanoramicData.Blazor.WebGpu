using System.Numerics;

namespace PanoramicData.Blazor.WebGpu.Camera;

/// <summary>
/// Abstract base class for all camera types.
/// Provides common functionality for view and projection matrix calculations.
/// </summary>
public abstract class PDWebGpuCameraBase
{
	private float _aspectRatio = 16f / 9f;
	private float _nearPlane = 0.1f;
	private float _farPlane = 1000f;
	private bool _viewMatrixDirty = true;
	private bool _projectionMatrixDirty = true;
	private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
	private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

	/// <summary>
	/// Gets or sets the aspect ratio (width / height).
	/// </summary>
	public float AspectRatio
	{
		get => _aspectRatio;
		set
		{
			if (_aspectRatio != value)
			{
				_aspectRatio = value;
				_projectionMatrixDirty = true;
			}
		}
	}

	/// <summary>
	/// Gets or sets the near clipping plane distance.
	/// </summary>
	public float NearPlane
	{
		get => _nearPlane;
		set
		{
			if (_nearPlane != value)
			{
				_nearPlane = value;
				_projectionMatrixDirty = true;
			}
		}
	}

	/// <summary>
	/// Gets or sets the far clipping plane distance.
	/// </summary>
	public float FarPlane
	{
		get => _farPlane;
		set
		{
			if (_farPlane != value)
			{
				_farPlane = value;
				_projectionMatrixDirty = true;
			}
		}
	}

	/// <summary>
	/// Gets the view matrix.
	/// </summary>
	public Matrix4x4 ViewMatrix
	{
		get
		{
			if (_viewMatrixDirty)
			{
				_viewMatrix = CalculateViewMatrix();
				_viewMatrixDirty = false;
			}
			return _viewMatrix;
		}
	}

	/// <summary>
	/// Gets the projection matrix.
	/// </summary>
	public Matrix4x4 ProjectionMatrix
	{
		get
		{
			if (_projectionMatrixDirty)
			{
				_projectionMatrix = CalculateProjectionMatrix();
				_projectionMatrixDirty = false;
			}
			return _projectionMatrix;
		}
	}

	/// <summary>
	/// Gets the combined view-projection matrix.
	/// </summary>
	public Matrix4x4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;

	/// <summary>
	/// Marks the view matrix as dirty, forcing recalculation on next access.
	/// </summary>
	protected void MarkViewMatrixDirty()
	{
		_viewMatrixDirty = true;
	}

	/// <summary>
	/// Marks the projection matrix as dirty, forcing recalculation on next access.
	/// </summary>
	protected void MarkProjectionMatrixDirty()
	{
		_projectionMatrixDirty = true;
	}

	/// <summary>
	/// Updates the camera state. Called each frame if automatic updates are enabled.
	/// </summary>
	/// <param name="deltaTime">Time since last update in seconds.</param>
	public virtual void Update(float deltaTime)
	{
		// Override in derived classes for camera-specific updates
	}

	/// <summary>
	/// Calculates the view matrix based on camera position and orientation.
	/// </summary>
	/// <returns>The view matrix.</returns>
	protected abstract Matrix4x4 CalculateViewMatrix();

	/// <summary>
	/// Calculates the projection matrix based on camera properties.
	/// </summary>
	/// <returns>The projection matrix.</returns>
	protected abstract Matrix4x4 CalculateProjectionMatrix();
}
