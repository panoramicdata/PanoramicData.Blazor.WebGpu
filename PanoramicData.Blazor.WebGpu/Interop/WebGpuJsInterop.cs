using Microsoft.JSInterop;

namespace PanoramicData.Blazor.WebGpu.Interop;

/// <summary>
/// Interface for components that want to receive page visibility change notifications.
/// </summary>
public interface IVisibilityCallback
{
	/// <summary>
	/// Called when the page visibility changes.
	/// </summary>
	/// <param name="isVisible">True if the page is visible; otherwise, false.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	[JSInvokable]
	Task OnVisibilityChanged(bool isVisible);
}

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
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Gets detailed compatibility information about the browser and WebGPU support.
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
			return new WebGpuCompatibilityInfo
			{
				IsSupported = false,
				ErrorMessage = $"Failed to detect WebGPU support: {ex.Message}",
				UserAgent = "unknown",
				Vendor = "unknown",
				Platform = "unknown"
			};
		}
	}

	/// <summary>
	/// Initializes the WebGPU device for the specified canvas.
	/// </summary>
	/// <param name="canvasId">The canvas element ID.</param>
	/// <returns>Device information.</returns>
	/// <exception cref="PDWebGpuNotSupportedException">Thrown when WebGPU is not supported.</exception>
	/// <exception cref="PDWebGpuDeviceException">Thrown when device initialization fails.</exception>
	public async ValueTask<WebGpuDeviceInfo> InitializeAsync(string canvasId)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<WebGpuDeviceInfo>("initializeAsync", canvasId);
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
	/// Creates a texture view from a texture.
	/// </summary>
	/// <param name="textureId">The texture resource ID.</param>
	/// <param name="descriptor">Optional view descriptor.</param>
	/// <returns>Resource ID for the texture view.</returns>
	public async ValueTask<int> CreateTextureViewAsync(int textureId, object? descriptor = null)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("createTextureView", textureId, descriptor);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to create texture view", ex);
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
	/// Registers a .NET callback for page visibility changes.
	/// </summary>
	/// <param name="dotNetRef">The .NET object reference.</param>
	/// <returns>Callback ID for later removal.</returns>
	public async ValueTask<int> RegisterVisibilityCallbackAsync(DotNetObjectReference<IVisibilityCallback> dotNetRef)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("registerVisibilityCallback", dotNetRef);
		}
		catch (Exception ex)
		{
			throw new PDWebGpuException("Failed to register visibility callback", ex);
		}
	}

	/// <summary>
	/// Unregisters a visibility callback.
	/// </summary>
	/// <param name="callbackId">The callback ID to remove.</param>
	public async ValueTask UnregisterVisibilityCallbackAsync(int callbackId)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("unregisterVisibilityCallback", callbackId);
		}
		catch
		{
			// Ignore errors during cleanup
		}
	}

	/// <summary>
	/// Checks if the page is currently visible.
	/// </summary>
	/// <returns>True if the page is visible.</returns>
	public async ValueTask<bool> IsPageVisibleAsync()
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<bool>("isPageVisible");
		}
		catch (Exception ex)
		{
			throw new PDWebGpuException("Failed to check page visibility", ex);
		}
	}

	/// <summary>
	/// Creates a buffer with initial data.
	/// </summary>
	/// <param name="data">The buffer data.</param>
	/// <param name="usage">Buffer usage flags.</param>
	/// <param name="label">Optional label for debugging.</param>
	/// <returns>Resource ID for the buffer.</returns>
	public async ValueTask<int> CreateBufferAsync(byte[] data, int usage, string? label = null)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("createBuffer", data, usage, label);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to create buffer", ex);
		}
	}

	/// <summary>
	/// Writes data to an existing buffer.
	/// </summary>
	/// <param name="bufferId">The buffer resource ID.</param>
	/// <param name="data">The data to write.</param>
	/// <param name="offset">Offset in bytes.</param>
	public async ValueTask WriteBufferAsync(int bufferId, byte[] data, long offset = 0)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("writeBuffer", bufferId, data, offset);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to write buffer", ex);
		}
	}

	/// <summary>
	/// Creates a render pipeline.
	/// </summary>
	/// <param name="descriptor">Pipeline descriptor.</param>
	/// <returns>Resource ID for the pipeline.</returns>
	public async ValueTask<int> CreateRenderPipelineAsync(object descriptor)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("createRenderPipeline", descriptor);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to create render pipeline", ex);
		}
	}

	/// <summary>
	/// Creates a bind group.
	/// </summary>
	/// <param name="descriptor">Bind group descriptor.</param>
	/// <returns>Resource ID for the bind group.</returns>
	public async ValueTask<int> CreateBindGroupAsync(object descriptor)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("createBindGroup", descriptor);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to create bind group", ex);
		}
	}

	/// <summary>
	/// Creates a command encoder.
	/// </summary>
	/// <param name="label">Optional label for debugging.</param>
	/// <returns>Resource ID for the command encoder.</returns>
	public async ValueTask<int> CreateCommandEncoderAsync(string? label = null)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("createCommandEncoder", label);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to create command encoder", ex);
		}
	}

	/// <summary>
	/// Begins a render pass.
	/// </summary>
	/// <param name="encoderId">Command encoder resource ID.</param>
	/// <param name="descriptor">Render pass descriptor.</param>
	/// <returns>Resource ID for the render pass encoder.</returns>
	public async ValueTask<int> BeginRenderPassAsync(int encoderId, object descriptor)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("beginRenderPass", encoderId, descriptor);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to begin render pass", ex);
		}
	}

	/// <summary>
	/// Sets the pipeline for a render pass.
	/// </summary>
	/// <param name="passEncoderId">Render pass encoder resource ID.</param>
	/// <param name="pipelineId">Pipeline resource ID.</param>
	public async ValueTask SetPipelineAsync(int passEncoderId, int pipelineId)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("setPipeline", passEncoderId, pipelineId);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to set pipeline", ex);
		}
	}

	/// <summary>
	/// Sets a bind group for a render pass.
	/// </summary>
	/// <param name="passEncoderId">Render pass encoder resource ID.</param>
	/// <param name="index">Bind group index.</param>
	/// <param name="bindGroupId">Bind group resource ID.</param>
	public async ValueTask SetBindGroupAsync(int passEncoderId, int index, int bindGroupId)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("setBindGroup", passEncoderId, index, bindGroupId);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to set bind group", ex);
		}
	}

	/// <summary>
	/// Sets a vertex buffer for a render pass.
	/// </summary>
	/// <param name="passEncoderId">Render pass encoder resource ID.</param>
	/// <param name="slot">Vertex buffer slot.</param>
	/// <param name="bufferId">Buffer resource ID.</param>
	public async ValueTask SetVertexBufferAsync(int passEncoderId, int slot, int bufferId)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("setVertexBuffer", passEncoderId, slot, bufferId);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to set vertex buffer", ex);
		}
	}

	/// <summary>
	/// Sets an index buffer for a render pass.
	/// </summary>
	/// <param name="passEncoderId">Render pass encoder resource ID.</param>
	/// <param name="bufferId">Buffer resource ID.</param>
	/// <param name="format">Index format ('uint16' or 'uint32').</param>
	public async ValueTask SetIndexBufferAsync(int passEncoderId, int bufferId, string format)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("setIndexBuffer", passEncoderId, bufferId, format);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to set index buffer", ex);
		}
	}

	/// <summary>
	/// Draws vertices.
	/// </summary>
	public async ValueTask DrawAsync(int passEncoderId, int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("draw", passEncoderId, vertexCount, instanceCount, firstVertex, firstInstance);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to draw", ex);
		}
	}

	/// <summary>
	/// Draws indexed vertices.
	/// </summary>
	public async ValueTask DrawIndexedAsync(int passEncoderId, int indexCount, int instanceCount = 1, int firstIndex = 0, int baseVertex = 0, int firstInstance = 0)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("drawIndexed", passEncoderId, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to draw indexed", ex);
		}
	}

	/// <summary>
	/// Ends a render pass.
	/// </summary>
	/// <param name="passEncoderId">Render pass encoder resource ID.</param>
	public async ValueTask EndRenderPassAsync(int passEncoderId)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("endRenderPass", passEncoderId);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to end render pass", ex);
		}
	}

	/// <summary>
	/// Finishes command encoder and returns command buffer.
	/// </summary>
	/// <param name="encoderId">Command encoder resource ID.</param>
	/// <returns>Resource ID for the command buffer.</returns>
	public async ValueTask<int> FinishCommandEncoderAsync(int encoderId)
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<int>("finishCommandEncoder", encoderId);
		}
		catch (JSException ex)
		{
			throw new PDWebGpuException("Failed to finish command encoder", ex);
		}
	}

	/// <summary>
	/// Gets current error statistics from the JavaScript interop layer.
	/// </summary>
	/// <returns>Error statistics including counts and details.</returns>
	public async ValueTask<WebGpuErrorStatistics> GetErrorStatisticsAsync()
	{
		try
		{
			var module = await _moduleTask.Value;
			return await module.InvokeAsync<WebGpuErrorStatistics>("getErrorStatistics");
		}
		catch (Exception ex)
		{
			// If we can't get error stats, return empty stats
			return new WebGpuErrorStatistics
			{
				UniqueErrors = 0,
				TotalErrors = 0,
				Errors = []
			};
		}
	}

	/// <summary>
	/// Clears all tracked error statistics.
	/// </summary>
	public async ValueTask ClearErrorStatisticsAsync()
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("clearErrorStatistics");
		}
		catch
		{
			// Ignore errors during cleanup
		}
	}

	/// <summary>
	/// Sets the interval at which error summaries are reported to the console.
	/// </summary>
	/// <param name="intervalMs">Interval in milliseconds (minimum 1000ms).</param>
	public async ValueTask SetErrorReportIntervalAsync(int intervalMs)
	{
		try
		{
			var module = await _moduleTask.Value;
			await module.InvokeVoidAsync("setErrorReportInterval", intervalMs);
		}
		catch (Exception ex)
		{
			throw new PDWebGpuException("Failed to set error report interval", ex);
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

/// <summary>
/// WebGPU error statistics from the JavaScript layer.
/// </summary>
public class WebGpuErrorStatistics
{
	/// <summary>
	/// Gets or sets the number of unique error types.
	/// </summary>
	public int UniqueErrors { get; set; }

	/// <summary>
	/// Gets or sets the total number of errors.
	/// </summary>
	public int TotalErrors { get; set; }

	/// <summary>
	/// Gets or sets the list of errors with their counts.
	/// </summary>
	public WebGpuErrorEntry[] Errors { get; set; } = [];
}

/// <summary>
/// Individual error entry in error statistics.
/// </summary>
public class WebGpuErrorEntry
{
	/// <summary>
	/// Gets or sets the error message.
	/// </summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the number of times this error occurred.
	/// </summary>
	public int Count { get; set; }
}
