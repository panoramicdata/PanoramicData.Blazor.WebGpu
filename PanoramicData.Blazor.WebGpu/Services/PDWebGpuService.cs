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
	public async Task<bool> IsSupportedAsync()
	{
		try
		{
			// Don't use cached value - always check fresh
			var isSupported = await _interop.IsSupportedAsync();
			_isSupported = isSupported;
			return isSupported;
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(new PDWebGpuException("Failed to check WebGPU support", ex)));
			_isSupported = false;
			return false;
		}
	}

	/// <inheritdoc/>
	public async Task<Interop.WebGpuCompatibilityInfo> GetCompatibilityInfoAsync()
	{
		try
		{
			// Always get fresh compatibility info
			var compatInfo = await _interop.GetCompatibilityInfoAsync();
			_compatibilityInfo = compatInfo;
			_isSupported = compatInfo.IsSupported;
			return compatInfo;
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(new PDWebGpuException("Failed to get compatibility information", ex)));
			throw;
		}
	}

	/// <inheritdoc/>
	public async Task InitializeAsync(string? canvasId = null)
	{
		if (_isInitialized)
		{
			return;
		}

		try
		{
			// Check WebGPU support first
			var compatibilityInfo = await _interop.GetCompatibilityInfoAsync();
			if (!compatibilityInfo.IsSupported)
			{
				throw new PDWebGpuNotSupportedException(compatibilityInfo);
			}

			// Initialize device
			var deviceInfo = await _interop.InitializeAsync(canvasId ?? "webgpu-canvas");
			_deviceInfo = deviceInfo;
			_compatibilityInfo = compatibilityInfo;
			_isSupported = true;
			_isInitialized = true;

			// Raise DeviceReady event
			DeviceReady?.Invoke(this, EventArgs.Empty);
		}
		catch (PDWebGpuNotSupportedException)
		{
			throw; // Re-throw with compatibility info
		}
		catch (Exception ex)
		{
			Error?.Invoke(this, new PDWebGpuErrorEventArgs(ex));
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
	/// <param name="name">Optional name for the shader for debugging purposes.</param>
	/// <returns>A PDWebGpuShader instance.</returns>
	public async Task<PDWebGpuShader> CreateShaderAsync(string wgslCode, string? name = null)
	{
		var resourceId = await CreateShaderModuleAsync(wgslCode);
		return new PDWebGpuShader(this, resourceId, wgslCode, name);
	}

	/// <inheritdoc/>
	public async Task<PDWebGpuBuffer> CreateBufferAsync(byte[] data, BufferType bufferType, string? name = null)
	{
		if (data == null || data.Length == 0)
		{
			throw new ArgumentException("Buffer data cannot be null or empty", nameof(data));
		}

		await EnsureInitializedAsync();

		try
		{
			// Map buffer type to WebGPU usage flags
			// GPUBufferUsage values: VERTEX = 0x20, INDEX = 0x10, UNIFORM = 0x40, STORAGE = 0x80, COPY_DST = 0x8
			int usage = bufferType switch
			{
				BufferType.Vertex => 0x20,  // GPUBufferUsage.VERTEX
				BufferType.Index => 0x10,   // GPUBufferUsage.INDEX
				BufferType.Uniform => 0x40, // GPUBufferUsage.UNIFORM
				BufferType.Storage => 0x80, // GPUBufferUsage.STORAGE
				_ => throw new ArgumentException($"Unsupported buffer type: {bufferType}", nameof(bufferType))
			};

			var resourceId = await _interop.CreateBufferAsync(data, usage, name);
			return new PDWebGpuBuffer(this, resourceId, data.Length, bufferType, name);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw new PDWebGpuException($"Failed to create {bufferType} buffer", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<PDWebGpuBuffer> CreateBufferAsync(float[] data, BufferType bufferType, string? name = null)
	{
		if (data == null || data.Length == 0)
		{
			throw new ArgumentException("Buffer data cannot be null or empty", nameof(data));
		}

		// Convert float[] to byte[]
		var bytes = new byte[data.Length * sizeof(float)];
		Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

		return await CreateBufferAsync(bytes, bufferType, name);
	}

	/// <inheritdoc/>
	public async Task<PDWebGpuBuffer> CreateBufferAsync(ushort[] data, BufferType bufferType, string? name = null)
	{
		if (data == null || data.Length == 0)
		{
			throw new ArgumentException("Buffer data cannot be null or empty", nameof(data));
		}

		// Convert ushort[] to byte[]
		var bytes = new byte[data.Length * sizeof(ushort)];
		Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

		return await CreateBufferAsync(bytes, bufferType, name);
	}

	/// <inheritdoc/>
	public async Task<PDWebGpuCommandEncoder> CreateCommandEncoderAsync(string? name = null)
	{
		await EnsureInitializedAsync();

		try
		{
			var resourceId = await _interop.CreateCommandEncoderAsync(name);
			return new PDWebGpuCommandEncoder(this, resourceId, _interop);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw new PDWebGpuException("Failed to create command encoder", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<PDWebGpuPipeline> CreateRenderPipelineAsync(RenderPipelineDescriptor descriptor, string? name = null)
	{
		if (descriptor == null)
		{
			throw new ArgumentNullException(nameof(descriptor));
		}

		await EnsureInitializedAsync();

		try
		{
			// Convert descriptor to JavaScript-compatible format
			var jsDescriptor = ConvertRenderPipelineDescriptor(descriptor, name);
			var resourceId = await _interop.CreateRenderPipelineAsync(jsDescriptor);
			return new PDWebGpuPipeline(this, resourceId, PipelineType.Render, name);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw new PDWebGpuException("Failed to create render pipeline", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<PDWebGpuBindGroup> CreateBindGroupAsync(BindGroupDescriptor descriptor, string? name = null)
	{
		if (descriptor == null)
		{
			throw new ArgumentNullException(nameof(descriptor));
		}

		await EnsureInitializedAsync();

		try
		{
			// Convert descriptor to JavaScript-compatible format
			var jsDescriptor = ConvertBindGroupDescriptor(descriptor, name);
			var resourceId = await _interop.CreateBindGroupAsync(jsDescriptor);
			return new PDWebGpuBindGroup(this, resourceId, name);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw new PDWebGpuException("Failed to create bind group", ex);
		}
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
	/// Gets the current texture from a canvas context.
	/// </summary>
	/// <param name="contextId">The canvas context ID.</param>
	/// <returns>Resource ID for the texture.</returns>
	public async Task<int> GetCurrentTextureAsync(string contextId)
	{
		if (string.IsNullOrWhiteSpace(contextId))
		{
			throw new ArgumentException("Context ID cannot be null or empty", nameof(contextId));
		}

		await EnsureInitializedAsync();

		try
		{
			return await _interop.GetCurrentTextureAsync(contextId);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
		}
	}

	/// <summary>
	/// Creates a texture view from a texture.
	/// </summary>
	/// <param name="textureId">The texture resource ID.</param>
	/// <param name="descriptor">Optional view descriptor.</param>
	/// <returns>Resource ID for the texture view.</returns>
	public async Task<int> CreateTextureViewAsync(int textureId, object? descriptor = null)
	{
		await EnsureInitializedAsync();

		try
		{
			return await _interop.CreateTextureViewAsync(textureId, descriptor);
		}
		catch (Exception ex)
		{
			OnError(new PDWebGpuErrorEventArgs(ex));
			throw;
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

	private static object ConvertRenderPipelineDescriptor(RenderPipelineDescriptor descriptor, string? name)
	{
		var result = new Dictionary<string, object?>
		{
			["label"] = name
		};

		// Vertex state
		if (descriptor.Vertex != null)
		{
			result["vertex"] = new
			{
				shaderModuleId = descriptor.Vertex.Shader?.ResourceId,
				entryPoint = descriptor.Vertex.EntryPoint ?? "main",
				buffers = descriptor.Vertex.Buffers?.Select(b => new
				{
					arrayStride = b.ArrayStride,
					stepMode = b.StepMode,
					attributes = b.Attributes?.Select(a => new
					{
						format = a.Format,
						offset = a.Offset,
						shaderLocation = a.ShaderLocation
					}).ToArray()
				}).ToArray()
			};
		}

		// Fragment state
		if (descriptor.Fragment != null)
		{
			result["fragment"] = new
			{
				shaderModuleId = descriptor.Fragment.Shader?.ResourceId,
				entryPoint = descriptor.Fragment.EntryPoint ?? "main",
				targets = descriptor.Fragment.Targets?.Select(t =>
				{
					var target = new Dictionary<string, object>
					{
						["format"] = t.Format,
						["writeMask"] = t.WriteMask
					};

					// Only include blend if it's not null
					if (t.Blend != null)
					{
						target["blend"] = new
						{
							color = t.Blend.Color != null ? new
							{
								srcFactor = t.Blend.Color.SrcFactor,
								dstFactor = t.Blend.Color.DstFactor,
								operation = t.Blend.Color.Operation
							} : null,
							alpha = t.Blend.Alpha != null ? new
							{
								srcFactor = t.Blend.Alpha.SrcFactor,
								dstFactor = t.Blend.Alpha.DstFactor,
								operation = t.Blend.Alpha.Operation
							} : null
						};
					}

					return target;
				}).ToArray()
			};
		}

		// Primitive state
		if (descriptor.Primitive != null)
		{
			var primitive = new Dictionary<string, object>
			{
				["topology"] = descriptor.Primitive.Topology
			};

			// Only include optional properties if they have values
			if (!string.IsNullOrEmpty(descriptor.Primitive.StripIndexFormat))
			{
				primitive["stripIndexFormat"] = descriptor.Primitive.StripIndexFormat;
			}
			if (!string.IsNullOrEmpty(descriptor.Primitive.FrontFace))
			{
				primitive["frontFace"] = descriptor.Primitive.FrontFace;
			}
			if (!string.IsNullOrEmpty(descriptor.Primitive.CullMode))
			{
				primitive["cullMode"] = descriptor.Primitive.CullMode;
			}

			result["primitive"] = primitive;
		}

		// Depth/stencil state
		if (descriptor.DepthStencil != null)
		{
			result["depthStencil"] = new
			{
				format = descriptor.DepthStencil.Format,
				depthWriteEnabled = descriptor.DepthStencil.DepthWriteEnabled,
				depthCompare = descriptor.DepthStencil.DepthCompare,
				stencilFront = descriptor.DepthStencil.StencilFront,
				stencilBack = descriptor.DepthStencil.StencilBack,
				stencilReadMask = descriptor.DepthStencil.StencilReadMask,
				stencilWriteMask = descriptor.DepthStencil.StencilWriteMask,
				depthBias = descriptor.DepthStencil.DepthBias,
				depthBiasSlopeScale = descriptor.DepthStencil.DepthBiasSlopeScale,
				depthBiasClamp = descriptor.DepthStencil.DepthBiasClamp
			};
		}

		// Multisample state
		if (descriptor.Multisample != null)
		{
			result["multisample"] = new
			{
				count = descriptor.Multisample.Count,
				mask = descriptor.Multisample.Mask,
				alphaToCoverageEnabled = descriptor.Multisample.AlphaToCoverageEnabled
			};
		}

		return result;
	}

	private static object ConvertBindGroupDescriptor(BindGroupDescriptor descriptor, string? name)
	{
		return new
		{
			label = name,
			layoutId = descriptor.LayoutId,
			pipelineId = descriptor.PipelineId,
			groupIndex = descriptor.GroupIndex,
			entries = descriptor.Entries?.Select(e => new
			{
				binding = e.Binding,
				resourceId = e.ResourceId,
				resourceType = e.ResourceType
			}).ToArray()
		};
	}
}
