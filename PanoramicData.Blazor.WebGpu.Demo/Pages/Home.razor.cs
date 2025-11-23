using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor.WebGpu.Camera;
using PanoramicData.Blazor.WebGpu.Components;
using PanoramicData.Blazor.WebGpu.Demo.Shaders;
using PanoramicData.Blazor.WebGpu.Performance;
using PanoramicData.Blazor.WebGpu.Services;

namespace PanoramicData.Blazor.WebGpu.Demo.Pages;

/// <summary>
/// Main shader editor demo page with split-view layout.
/// </summary>
public partial class Home : IDisposable
{
	private PDWebGpuContainer? _container;
	private PDWebGpuCameraBase? _activeCamera;
	private PDWebGpuOrbitCamera? _orbitCamera;
	private PDWebGpuFirstPersonCamera? _firstPersonCamera;
	private PDWebGpuOrthographicCamera? _orthoCamera;

	private string _currentShaderCode = ExampleShaders.SimpleTriangleVertex;
	private string? _compilationError;
	private bool _compilationSuccess;
	private bool _isCompiling;
	private bool _showPerformance;
	private bool? _webGpuSupported;
	private string? _deviceError;
	private CameraMode _cameraMode = CameraMode.Orbit;

	private readonly Dictionary<string, string> _exampleShaders = ExampleShaders.GetAllShaders();

	private readonly PDWebGpuPerformanceDisplayOptions _performanceOptions = new()
	{
		ShowFPS = true,
		ShowFrameTime = true,
		ShowFrameTimeUsage = true,
		ShowDrawCalls = false,
		ShowTriangleCount = false,
		Position = CornerPosition.TopRight,
		UpdateIntervalMs = 500
	};

	protected override async Task OnInitializedAsync()
	{
		// Initialize cameras
		_orbitCamera = new PDWebGpuOrbitCamera
		{
			Target = System.Numerics.Vector3.Zero,
			Distance = 5.0f
		};

		_firstPersonCamera = new PDWebGpuFirstPersonCamera
		{
			Position = new System.Numerics.Vector3(0, 0, 5),
			MoveSpeed = 2.0f
		};

		_orthoCamera = new PDWebGpuOrthographicCamera
		{
			Left = -2.0f,
			Right = 2.0f,
			Bottom = -2.0f,
			Top = 2.0f
		};

		SetCameraMode(CameraMode.Orbit);

		// Check WebGPU support
		_webGpuSupported = await WebGpuService.IsSupportedAsync();
	}

	private async Task OnGpuReady(EventArgs args)
	{
		// GPU is initialized and ready for rendering
		await InvokeAsync(StateHasChanged);
	}

	private async Task OnError(PDWebGpuErrorEventArgs args)
	{
		_deviceError = args.Message;
		await InvokeAsync(StateHasChanged);
	}

	private async Task OnFrame(PDWebGpuFrameEventArgs args)
	{
		// Update camera
		if (_activeCamera != null)
		{
			// Simple rotation for orbit camera
			if (_cameraMode == CameraMode.Orbit && _activeCamera is PDWebGpuOrbitCamera orbitCam)
			{
				orbitCam.Rotate((float)args.DeltaTime * 0.2f, 0);
			}
		}

		// TODO: Actual rendering logic will be implemented in a future phase
		// For now, the canvas is initialized and the render loop is running
		// This demonstrates the framework structure working correctly

		await Task.CompletedTask;
	}

	private async Task CompileShader()
	{
		_isCompiling = true;
		_compilationError = null;
		_compilationSuccess = false;
		StateHasChanged();

		try
		{
			// Validate shader first
			var validation = Resources.PDWebGpuShader.Validate(_currentShaderCode);
			if (!validation.Success)
			{
				_compilationError = validation.ErrorMessage ?? "Unknown validation error";
				return;
			}

			// In a future phase, this will actually compile and use the shader
			// For now, just validate it
			_compilationSuccess = true;
			await Task.Delay(3000); // Show success message for 3 seconds
			_compilationSuccess = false;
		}
		catch (Exception ex)
		{
			_compilationError = ex.Message;
		}
		finally
		{
			_isCompiling = false;
			StateHasChanged();
		}
	}

	private void TogglePerformanceMetrics()
	{
		_showPerformance = !_showPerformance;
	}

	private void LoadExampleShader(ChangeEventArgs args)
	{
		var selectedExample = args.Value?.ToString();
		if (!string.IsNullOrEmpty(selectedExample) && _exampleShaders.TryGetValue(selectedExample, out var value))
		{
			_currentShaderCode = value;
		}
	}

	private void SetCameraMode(CameraMode mode)
	{
		_cameraMode = mode;
		_activeCamera = mode switch
		{
			CameraMode.Orbit => _orbitCamera,
			CameraMode.FirstPerson => _firstPersonCamera,
			CameraMode.Orthographic => _orthoCamera,
			_ => _orbitCamera
		};

		// Update aspect ratio for all cameras
		// Canvas dimensions will be handled by the component itself
		// For now, use a default aspect ratio
		var aspectRatio = 16f / 9f; // Default aspect ratio
		if (_orbitCamera != null) _orbitCamera.AspectRatio = aspectRatio;
		if (_firstPersonCamera != null) _firstPersonCamera.AspectRatio = aspectRatio;
		if (_orthoCamera != null)
		{
			_orthoCamera.Left = -2.0f * aspectRatio;
			_orthoCamera.Right = 2.0f * aspectRatio;
		}
	}

	public void Dispose()
	{
		// Resources will be disposed in future phases when rendering is implemented
	}
}

/// <summary>
/// Camera modes for the demo.
/// </summary>
public enum CameraMode
{
	Orbit,
	FirstPerson,
	Orthographic
}
