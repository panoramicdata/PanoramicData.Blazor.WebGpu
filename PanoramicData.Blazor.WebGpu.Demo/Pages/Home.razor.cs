using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor.WebGpu.Camera;
using PanoramicData.Blazor.WebGpu.Components;
using PanoramicData.Blazor.WebGpu.Demo.Shaders;
using PanoramicData.Blazor.WebGpu.Diagnostics;
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
			Top = 2.0f,
			NearPlane = 0.1f,
			FarPlane = 100.0f
		};

		// Set initial active camera
		_activeCamera = _orbitCamera;
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			// Check WebGPU support after first render to ensure JS is loaded
			try
			{
				_webGpuSupported = await WebGpuService.IsSupportedAsync();
				
				if (!_webGpuSupported.Value)
				{
					var compatInfo = await WebGpuService.GetCompatibilityInfoAsync();
					_deviceError = DiagnosticHelper.GetNotSupportedMessage(compatInfo);
				}
				
				StateHasChanged();
			}
			catch (Exception ex)
			{
				_webGpuSupported = false;
				_deviceError = $"Error checking WebGPU support: {ex.Message}\n\nStack: {ex.StackTrace}";
				StateHasChanged();
			}
		}
	}

	/// <summary>
	/// Called when Monaco editor is initialized.
	/// </summary>
	private Task OnMonacoInit()
	{
		// Placeholder for future Monaco integration
		return Task.CompletedTask;
	}

	/// <summary>
	/// Compiles the shader code in the editor.
	/// </summary>
	private async Task CompileShader()
	{
		_isCompiling = true;
		_compilationError = null;
		_compilationSuccess = false;

		try
		{
			// Validate shader
			var validationResult = Resources.PDWebGpuShader.Validate(_currentShaderCode);
			
			if (!validationResult.Success)
			{
				_compilationError = validationResult.ErrorMessage;
				StateHasChanged();
				return;
			}

			// Try to create shader module (this will compile it)
			await WebGpuService.CreateShaderAsync(_currentShaderCode);

			_compilationSuccess = true;
			
			// Clear success message after 3 seconds
			_ = Task.Run(async () =>
			{
				await Task.Delay(3000);
				_compilationSuccess = false;
				await InvokeAsync(StateHasChanged);
			});
		}
		catch (PDWebGpuShaderCompilationException ex)
		{
			_compilationError = DiagnosticHelper.GetShaderErrorMessage(ex);
		}
		catch (Exception ex)
		{
			_compilationError = $"Unexpected error: {ex.Message}";
		}
		finally
		{
			_isCompiling = false;
			StateHasChanged();
		}
	}

	/// <summary>
	/// Loads an example shader into the editor.
	/// </summary>
	private void LoadExampleShader(ChangeEventArgs e)
	{
		var shaderName = e.Value?.ToString();
		if (string.IsNullOrEmpty(shaderName))
		{
			return;
		}

		if (_exampleShaders.TryGetValue(shaderName, out var shaderCode))
		{
			_currentShaderCode = shaderCode;
			_compilationError = null;
			_compilationSuccess = false;
		}
	}

	/// <summary>
	/// Toggles the performance metrics display.
	/// </summary>
	private void TogglePerformanceMetrics()
	{
		_showPerformance = !_showPerformance;
	}

	/// <summary>
	/// Sets the active camera mode.
	/// </summary>
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
	}

	/// <summary>
	/// Called on each frame render.
	/// </summary>
	private void OnFrame(PDWebGpuFrameEventArgs args)
	{
		// TODO: Implement actual rendering logic
		// This is where you would:
		// 1. Update camera matrices
		// 2. Update uniforms (time, transformations)
		// 3. Execute render pipeline
		// 4. Draw geometry

		// For now, just update the active camera if needed
		if (_activeCamera != null)
		{
			// Camera matrices are automatically calculated via properties
			_ = _activeCamera.ViewProjectionMatrix;
		}
	}

	/// <summary>
	/// Called when GPU is ready.
	/// </summary>
	private void OnGpuReady(EventArgs args)
	{
		// GPU initialized successfully
		_webGpuSupported = true;
		StateHasChanged();
	}

	/// <summary>
	/// Called when an error occurs.
	/// </summary>
	private void OnError(PDWebGpuErrorEventArgs args)
	{
		_deviceError = args.Exception is PDWebGpuDeviceException deviceEx
			? DiagnosticHelper.GetDeviceErrorMessage(deviceEx)
			: $"Error: {args.Message}";
		
		StateHasChanged();
	}

	/// <summary>
	/// Disposes resources.
	/// </summary>
	public void Dispose()
	{
		// Cleanup will be handled by component disposal
		GC.SuppressFinalize(this);
	}
}

/// <summary>
/// Camera mode enumeration.
/// </summary>
public enum CameraMode
{
	Orbit,
	FirstPerson,
	Orthographic
}
