using Microsoft.JSInterop;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Resources;

namespace PanoramicData.Blazor.WebGpu.Services;

/// <summary>
/// Implementation of the WebGPU service for managing device lifecycle and operations.
/// </summary>
public class PDWebGpuService : IPDWebGpuService, IDisposable
{
	private readonly WebGpuJsInterop _interop;
	private bool _isInitialized;
	private bool _isSupported;
	private WebGpuDeviceInfo? _deviceInfo;
	private WebGpuCompatibilityInfo? _compatibilityInfo;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuService"/> class.
	/// </summary>
	/// <param name="jsRuntime">The JavaScript runtime.</param>
	public PDWebGpuService(IJSRuntime jsRuntime)
	{
		_interop = new WebGpuJsInterop(jsRuntime);
	}

	/// <inheritdoc/>
	public bool IsSupported => _isSupported;

	/// <inheritdoc/>
	public bool IsInitialized => _isInitialized;

	/// <inheritdoc/>
	public WebGpuDeviceInfo? DeviceInfo => _deviceInfo;

	/// <inheritdoc/>
	public WebGpuCompatibilityInfo? CompatibilityInfo => _compatibilityInfo;

	/// <inheritdoc/>
	public event EventHandler<EventArgs>? DeviceReady;

	/// <inheritdoc/>
	public event EventHandler<PDWebGpuDeviceLostEventArgs>? DeviceLost;

	/// <inheritdoc/>
	public event EventHandler<PDWebGpuErrorEventArgs>? Error;

	/// <inheritdoc/>
	public async Task<bool> CheckSupportAsync()
	{
		try
		{
			_isSupported = await _interop.IsSupportedAsync();
			return _isSupported;
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			return false;
		}
	}

	/// <inheritdoc/>
	public async Task<WebGpuCompatibilityInfo> GetCompatibilityInfoAsync()
	{
		try
		{
			_compatibilityInfo = await _interop.GetCompatibilityInfoAsync();
			_isSupported = _compatibilityInfo.IsSupported;
			return _compatibilityInfo;
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
		}
	}

	/// <inheritdoc/>
	public async Task InitializeAsync(string powerPreference = "high-performance", string[]? requiredFeatures = null)
	{
		if (_isInitialized)
		{
			return;
		}

		try
		{
			// Check support first
			if (!_isSupported)
			{
				await CheckSupportAsync();
			}

			if (!_isSupported)
			{
				_compatibilityInfo ??= await GetCompatibilityInfoAsync();
				throw new PDWebGpuNotSupportedException(_compatibilityInfo);
			}

			// Initialize the device
			var options = new WebGpuInitOptions
			{
				PowerPreference = powerPreference,
				RequiredFeatures = requiredFeatures ?? []
			};

			_deviceInfo = await _interop.InitializeAsync(options);
			_isInitialized = true;

			// Raise the DeviceReady event
			OnDeviceReady();
		}
		catch (PDWebGpuException)
		{
			throw;
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw new PDWebGpuDeviceException("Failed to initialize WebGPU device", ex);
		}
	}

	/// <inheritdoc/>
	public async Task EnsureInitializedAsync()
	{
		if (!_isInitialized)
		{
			await InitializeAsync();
		}
	}

	/// <inheritdoc/>
	public async Task<string> GetCanvasContextAsync(string canvasId)
	{
		if (string.IsNullOrWhiteSpace(canvasId))
		{
			throw new ArgumentException("Canvas ID cannot be null or empty", nameof(canvasId));
		}

		await EnsureInitializedAsync();

		try
		{
			return await _interop.GetCanvasContextAsync(canvasId);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
		}
	}

	/// <inheritdoc/>
	public async Task ConfigureCanvasContextAsync(string contextId, string? format = null, string alphaMode = "opaque")
	{
		if (string.IsNullOrWhiteSpace(contextId))
		{
			throw new ArgumentException("Context ID cannot be null or empty", nameof(contextId));
		}

		await EnsureInitializedAsync();

		try
		{
			var config = new CanvasContextConfig
			{
				Format = format,
				AlphaMode = alphaMode
			};

			await _interop.ConfigureCanvasContextAsync(contextId, config);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
		}
	}

	/// <inheritdoc/>
	public async Task<int> CreateShaderModuleAsync(string wgslCode)
	{
		if (string.IsNullOrWhiteSpace(wgslCode))
		{
			throw new ArgumentException("WGSL code cannot be null or empty", nameof(wgslCode));
		}

		await EnsureInitializedAsync();

		try
		{
			return await _interop.CreateShaderModuleAsync(wgslCode);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
		}
	}

	/// <summary>
	/// Creates a shader module from WGSL source code and returns a wrapper object.
	/// </summary>
	/// <param name="wgslCode">The WGSL shader source code.</param>
	/// <returns>A PDWebGpuShader instance.</returns>
	public async Task<PDWebGpuShader> CreateShaderAsync(string wgslCode)
	{
		var resourceId = await CreateShaderModuleAsync(wgslCode);
		return new PDWebGpuShader(this, resourceId, wgslCode);
	}

	/// <inheritdoc/>
	public async Task SubmitCommandBuffersAsync(params int[] commandBufferIds)
	{
		if (commandBufferIds == null || commandBufferIds.Length == 0)
		{
			return;
		}

		await EnsureInitializedAsync();

		try
		{
			await _interop.SubmitCommandBuffersAsync(commandBufferIds);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
		}
	}

	/// <inheritdoc/>
	public async Task ReleaseResourceAsync(int resourceId)
	{
		try
		{
			await _interop.ReleaseResourceAsync(resourceId);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			// Don't throw during resource cleanup
		}
	}

	/// <summary>
	/// Raises the DeviceReady event.
	/// </summary>
	protected virtual void OnDeviceReady()
	{
		DeviceReady?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Raises the DeviceLost event.
	/// </summary>
	/// <param name="args">Event arguments.</param>
	protected virtual void OnDeviceLost(PDWebGpuDeviceLostEventArgs args)
	{
		_isInitialized = false;
		DeviceLost?.Invoke(this, args);
	}

	/// <summary>
	/// Raises the Error event.
	/// </summary>
	/// <param name="args">Event arguments.</param>
	protected virtual void OnError(PDWebGpuErrorEventArgs args)
	{
		Error?.Invoke(this, args);
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		try
		{
			await _interop.DisposeAsync();
		}
		catch
		{
			// Ignore errors during disposal
		}

		_isInitialized = false;
		_deviceInfo = null;

		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the service synchronously.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		DisposeAsync().AsTask().GetAwaiter().GetResult();
	}
}
