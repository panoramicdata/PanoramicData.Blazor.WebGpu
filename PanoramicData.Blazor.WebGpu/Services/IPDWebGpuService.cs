namespace PanoramicData.Blazor.WebGpu.Services;

/// <summary>
/// Primary service interface for WebGPU operations.
/// Manages device lifecycle, initialization, and resource coordination.
/// </summary>
public interface IPDWebGpuService : IAsyncDisposable
{
	/// <summary>
	/// Gets a value indicating whether WebGPU is supported in the current browser.
	/// </summary>
	bool IsSupported { get; }

	/// <summary>
	/// Gets a value indicating whether the WebGPU device has been initialized.
	/// </summary>
	bool IsInitialized { get; }

	/// <summary>
	/// Gets the device information after initialization.
	/// </summary>
	Interop.WebGpuDeviceInfo? DeviceInfo { get; }

	/// <summary>
	/// Gets the browser compatibility information.
	/// </summary>
	Interop.WebGpuCompatibilityInfo? CompatibilityInfo { get; }

	/// <summary>
	/// Event raised when the WebGPU device is initialized and ready.
	/// </summary>
	event EventHandler<EventArgs>? DeviceReady;

	/// <summary>
	/// Event raised when the WebGPU device is lost.
	/// </summary>
	event EventHandler<PDWebGpuDeviceLostEventArgs>? DeviceLost;

	/// <summary>
	/// Event raised when an error occurs.
	/// </summary>
	event EventHandler<PDWebGpuErrorEventArgs>? Error;

	/// <summary>
	/// Checks if WebGPU is supported in the current browser.
	/// </summary>
	/// <returns>True if WebGPU is supported; otherwise, false.</returns>
	Task<bool> IsSupportedAsync();

	/// <summary>
	/// Gets detailed compatibility information about the browser and WebGPU support.
	/// </summary>
	/// <returns>Compatibility information.</returns>
	Task<Interop.WebGpuCompatibilityInfo> GetCompatibilityInfoAsync();

	/// <summary>
	/// Initializes the WebGPU service and acquires the device.
	/// </summary>
	/// <param name="canvasId">Optional canvas ID.</param>
	Task InitializeAsync(string? canvasId = null);

	/// <summary>
	/// Ensures the WebGPU device is initialized, initializing it if necessary.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task EnsureInitializedAsync();

	/// <summary>
	/// Gets or creates a canvas context for the specified canvas ID.
	/// </summary>
	/// <param name="canvasId">The canvas element ID.</param>
	/// <returns>The canvas context ID.</returns>
	Task<string> GetCanvasContextAsync(string canvasId);

	/// <summary>
	/// Configures a canvas context with the specified format and options.
	/// </summary>
	/// <param name="contextId">The canvas context ID.</param>
	/// <param name="format">Texture format (optional, uses preferred format if not specified).</param>
	/// <param name="alphaMode">Alpha mode (opaque, premultiplied).</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task ConfigureCanvasContextAsync(string contextId, string? format = null, string alphaMode = "opaque");

	/// <summary>
	/// Creates a shader module from WGSL source code.
	/// </summary>
	/// <param name="wgslCode">The WGSL shader source code.</param>
	/// <returns>The resource ID of the created shader module.</returns>
	Task<int> CreateShaderModuleAsync(string wgslCode);

	/// <summary>
	/// Creates a shader module from WGSL source code and returns a wrapper object.
	/// </summary>
	/// <param name="wgslCode">The WGSL shader source code.</param>
	/// <param name="name">Optional name for the shader for debugging purposes.</param>
	/// <returns>A PDWebGpuShader instance.</returns>
	Task<Resources.PDWebGpuShader> CreateShaderAsync(string wgslCode, string? name = null);

	/// <summary>
	/// Creates a GPU buffer for vertex, index, uniform, or storage data.
	/// </summary>
	/// <param name="data">Initial buffer data.</param>
	/// <param name="bufferType">Type of buffer to create.</param>
	/// <param name="name">Optional name for the buffer for debugging purposes.</param>
	/// <returns>A PDWebGpuBuffer instance.</returns>
	Task<Resources.PDWebGpuBuffer> CreateBufferAsync(byte[] data, Resources.BufferType bufferType, string? name = null);

	/// <summary>
	/// Creates a GPU buffer for vertex, index, uniform, or storage data.
	/// </summary>
	/// <param name="data">Initial buffer data as floats.</param>
	/// <param name="bufferType">Type of buffer to create.</param>
	/// <param name="name">Optional name for the buffer for debugging purposes.</param>
	/// <returns>A PDWebGpuBuffer instance.</returns>
	Task<Resources.PDWebGpuBuffer> CreateBufferAsync(float[] data, Resources.BufferType bufferType, string? name = null);

	/// <summary>
	/// Creates a GPU buffer for vertex, index, uniform, or storage data.
	/// </summary>
	/// <param name="data">Initial buffer data as unsigned shorts.</param>
	/// <param name="bufferType">Type of buffer to create.</param>
	/// <param name="name">Optional name for the buffer for debugging purposes.</param>
	/// <returns>A PDWebGpuBuffer instance.</returns>
	Task<Resources.PDWebGpuBuffer> CreateBufferAsync(ushort[] data, Resources.BufferType bufferType, string? name = null);

	/// <summary>
	/// Creates a command encoder for recording GPU commands.
	/// </summary>
	/// <param name="name">Optional name for the encoder for debugging purposes.</param>
	/// <returns>A PDWebGpuCommandEncoder instance.</returns>
	Task<Resources.PDWebGpuCommandEncoder> CreateCommandEncoderAsync(string? name = null);

	/// <summary>
	/// Creates a render pipeline.
	/// </summary>
	/// <param name="descriptor">Pipeline configuration.</param>
	/// <param name="name">Optional name for the pipeline for debugging purposes.</param>
	/// <returns>A PDWebGpuPipeline instance.</returns>
	Task<Resources.PDWebGpuPipeline> CreateRenderPipelineAsync(Resources.RenderPipelineDescriptor descriptor, string? name = null);

	/// <summary>
	/// Creates a bind group for binding resources to shaders.
	/// </summary>
	/// <param name="descriptor">Bind group configuration.</param>
	/// <param name="name">Optional name for the bind group for debugging purposes.</param>
	/// <returns>A PDWebGpuBindGroup instance.</returns>
	Task<Resources.PDWebGpuBindGroup> CreateBindGroupAsync(Resources.BindGroupDescriptor descriptor, string? name = null);

	/// <summary>
	/// Submits command buffers to the GPU queue.
	/// </summary>
	/// <param name="commandBufferIds">Array of command buffer resource IDs.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task SubmitCommandBuffersAsync(params int[] commandBufferIds);

	/// <summary>
	/// Releases a GPU resource.
	/// </summary>
	/// <param name="resourceId">The resource ID to release.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task ReleaseResourceAsync(int resourceId);

	/// <summary>
	/// Gets the current texture from a canvas context.
	/// </summary>
	/// <param name="contextId">The canvas context ID.</param>
	/// <returns>Resource ID for the texture.</returns>
	Task<int> GetCurrentTextureAsync(string contextId);

	/// <summary>
	/// Creates a texture view from a texture.
	/// </summary>
	/// <param name="textureId">The texture resource ID.</param>
	/// <param name="descriptor">Optional view descriptor.</param>
	/// <returns>Resource ID for the texture view.</returns>
	Task<int> CreateTextureViewAsync(int textureId, object? descriptor = null);
}

/// <summary>
/// Event args for device lost events.
/// </summary>
public class PDWebGpuDeviceLostEventArgs : EventArgs
{
	/// <summary>
	/// Gets or sets the reason the device was lost.
	/// </summary>
	public string Reason { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets additional message about the device loss.
	/// </summary>
	public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Event args for WebGPU error events.
/// </summary>
public class PDWebGpuErrorEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuErrorEventArgs"/> class.
	/// </summary>
	/// <param name="exception">The exception that occurred.</param>
	public PDWebGpuErrorEventArgs(Exception exception)
	{
		Exception = exception;
	}

	/// <summary>
	/// Gets the exception that occurred.
	/// </summary>
	public Exception Exception { get; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string Message => Exception.Message;
}
