using Microsoft.JSInterop;

namespace PanoramicData.Blazor.WebGpu.Interop;

/// <summary>
/// Provides JavaScript interop functionality for WebGPU operations.
/// This class wraps the webgpu-interop.js JavaScript module.
/// </summary>
internal sealed class WebGpuJsInterop : IAsyncDisposable
{
	private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="WebGpuJsInterop"/> class.
	/// </summary>
	/// <param name="jsRuntime">The JavaScript runtime.</param>
	public WebGpuJsInterop(IJSRuntime jsRuntime)
	{
		_moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
			jsRuntime.InvokeAsync<IJSObjectReference>(
				"import",
				"./_content/PanoramicData.Blazor.WebGpu/webgpu-interop.js").AsTask());
	}

	/// <summary>
	/// Checks if WebGPU is supported in the current browser.
	/// </summary>
	/// <returns>True if WebGPU is supported; otherwise, false.</returns>
	public async ValueTask<bool> IsSupportedAsync()
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<bool>("isSupported");
		}
		catch (Exception ex)
		{
			throw new PDWebGpuException("Failed to check WebGPU support", ex);
		}
	}

	/// <summary>
	/// Gets browser compatibility information for WebGPU.
	/// </summary>
	/// <returns>Compatibility information.</returns>
	public async ValueTask<WebGpuCompatibilityInfo> GetCompatibilityInfoAsync()
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<WebGpuCompatibilityInfo>("getCompatibilityInfo");
		}
		catch (Exception ex)
		{
			throw new PDWebGpuException("Failed to get compatibility information", ex);
		}
	}

	/// <summary>
	/// Initializes the WebGPU adapter and device.
	/// </summary>
	/// <param name="options">Initialization options.</param>
	/// <returns>Device information.</returns>
	public async ValueTask<WebGpuDeviceInfo> InitializeAsync(WebGpuInitOptions? options = null)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<WebGpuDeviceInfo>("initializeAsync", options ?? new WebGpuInitOptions());
		}
		catch (JSException ex)
		{
			throw new PDWebGpuDeviceException("Failed to initialize WebGPU device", ex);
		}
		catch (Exception ex)
		{
			throw new PDWebGpuException("Failed to initialize WebGPU", ex);
		}
	}

	/// <summary>
	/// Gets or creates a canvas context for WebGPU rendering.
	/// </summary>
	/// <param name="canvasId">The ID of the canvas element.</param>
	/// <returns>Canvas context information.</returns>
	public async ValueTask<string> GetCanvasContextAsync(string canvasId)
	{
		try
		{
			var module = await _moduleTask.Value;
			var result = await module.InvokeAsync<CanvasContextResult>("getCanvasContext", canvasId);
			return result.ContextId;
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException($"Failed to get canvas context for '{canvasId}'", ex);
		}
	}

	/// <summary>
	/// Configures a canvas context.
	/// </summary>
	/// <param name="contextId">The canvas context ID.</param>
	/// <param name="config">Configuration options.</param>
	public async ValueTask ConfigureCanvasContextAsync(string contextId, CanvasContextConfig config)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("configureCanvasContext", contextId, config);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException($"Failed to configure canvas context '{contextId}'", ex);
		}
	}

	/// <summary>
	/// Gets the current texture from a canvas context.
	/// </summary>
	/// <param name="contextId">The canvas context ID.</param>
	/// <returns>Resource ID for the texture.</returns>
	public async ValueTask<int> GetCurrentTextureAsync(string contextId)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("getCurrentTexture", contextId);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException($"Failed to get current texture from context '{contextId}'", ex);
		}
	}

	/// <summary>
	/// Creates a shader module from WGSL source code.
	/// </summary>
	/// <param name="wgslCode">The WGSL shader source code.</param>
	/// <returns>Resource ID for the shader module.</returns>
	public async ValueTask<int> CreateShaderModuleAsync(string wgslCode)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("createShaderModuleAsync", wgslCode);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuShaderCompilationException("Shader compilation failed", wgslCode, ex);
		}
	}

	/// <summary>
	/// Submits command buffers to the device queue.
	/// </summary>
	/// <param name="commandBufferIds">Array of command buffer resource IDs.</param>
	public async ValueTask SubmitCommandBuffersAsync(int[] commandBufferIds)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("submitCommandBuffers", new object[] { commandBufferIds });
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to submit command buffers", ex);
		}
	}

	/// <summary>
	/// Releases a WebGPU resource.
	/// </summary>
	/// <param name="resourceId">The resource ID to release.</param>
	public async ValueTask ReleaseResourceAsync(int resourceId)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("releaseResource", resourceId);
		}
		catch
		{
			// Ignore errors during resource cleanup
		}
	}

	/// <summary>
	/// Disposes the JavaScript interop and cleans up resources.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		if (_moduleTask.IsValueCreated)
		{
			try
			{
				var module = await _moduleTask.Value;
				await module.InvokeVoidAsync("dispose");
				await module.DisposeAsync();
			}
			catch
			{
				// Ignore errors during disposal
			}
		}
	}
}

/// <summary>
/// WebGPU initialization options.
/// </summary>
internal class WebGpuInitOptions
{
	/// <summary>
	/// Gets or sets the power preference (default: "high-performance").
	/// </summary>
	public string PowerPreference { get; set; } = "high-performance";

	/// <summary>
	/// Gets or sets the required features.
	/// </summary>
	public string[] RequiredFeatures { get; set; } = [];

	/// <summary>
	/// Gets or sets the required limits.
	/// </summary>
	public Dictionary<string, int>? RequiredLimits { get; set; }
}

/// <summary>
/// Canvas context configuration.
/// </summary>
internal class CanvasContextConfig
{
	/// <summary>
	/// Gets or sets the texture format.
	/// </summary>
	public string? Format { get; set; }

	/// <summary>
	/// Gets or sets the texture usage flags.
	/// </summary>
	public int? Usage { get; set; }

	/// <summary>
	/// Gets or sets the alpha mode.
	/// </summary>
	public string? AlphaMode { get; set; }
}

/// <summary>
/// Canvas context result.
/// </summary>
internal class CanvasContextResult
{
	/// <summary>
	/// Gets or sets the context ID.
	/// </summary>
	public string ContextId { get; set; } = string.Empty;
}

/// <summary>
/// WebGPU compatibility information.
/// </summary>
public class WebGpuCompatibilityInfo
{
	/// <summary>
	/// Gets or sets whether WebGPU is supported.
	/// </summary>
	public bool IsSupported { get; set; }

	/// <summary>
	/// Gets or sets the user agent string.
	/// </summary>
	public string UserAgent { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the browser vendor.
	/// </summary>
	public string Vendor { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the platform.
	/// </summary>
	public string Platform { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the error message if WebGPU is not supported.
	/// </summary>
	public string? ErrorMessage { get; set; }
}

/// <summary>
/// WebGPU device information.
/// </summary>
public class WebGpuDeviceInfo
{
	/// <summary>
	/// Gets or sets the adapter information.
	/// </summary>
	public AdapterInfo AdapterInfo { get; set; } = new();

	/// <summary>
	/// Gets or sets the device limits.
	/// </summary>
	public object? Limits { get; set; }

	/// <summary>
	/// Gets or sets the supported features.
	/// </summary>
	public string[] Features { get; set; } = [];
}

/// <summary>
/// WebGPU adapter information.
/// </summary>
public class AdapterInfo
{
	/// <summary>
	/// Gets or sets the vendor name.
	/// </summary>
	public string Vendor { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the architecture.
	/// </summary>
	public string Architecture { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the device name.
	/// </summary>
	public string Device { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string Description { get; set; } = string.Empty;
}
