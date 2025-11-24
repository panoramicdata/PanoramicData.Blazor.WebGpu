namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Represents a WebGPU command encoder for recording GPU commands.
/// </summary>
public class PDWebGpuCommandEncoder : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private readonly Interop.WebGpuJsInterop _interop;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuCommandEncoder"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	/// <param name="interop">The JavaScript interop.</param>
	internal PDWebGpuCommandEncoder(Services.IPDWebGpuService service, int resourceId, Interop.WebGpuJsInterop interop)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_interop = interop ?? throw new ArgumentNullException(nameof(interop));
		_resourceId = resourceId;
	}

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the command encoder has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Begins a render pass and returns a render pass encoder.
	/// </summary>
	/// <param name="descriptor">Render pass descriptor.</param>
	/// <returns>Render pass encoder resource ID.</returns>
	public async Task<int> BeginRenderPassAsync(RenderPassDescriptor descriptor)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		if (descriptor == null)
		{
			throw new ArgumentNullException(nameof(descriptor));
		}

		// Convert descriptor to JavaScript-compatible format
		var jsDescriptor = ConvertRenderPassDescriptor(descriptor);
		return await _interop.BeginRenderPassAsync(_resourceId, jsDescriptor);
	}

	/// <summary>
	/// Sets the pipeline for a render pass.
	/// </summary>
	public async Task SetPipelineAsync(int passEncoderId, int pipelineId)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.SetPipelineAsync(passEncoderId, pipelineId);
	}

	/// <summary>
	/// Sets a bind group for a render pass.
	/// </summary>
	public async Task SetBindGroupAsync(int passEncoderId, int index, int bindGroupId)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.SetBindGroupAsync(passEncoderId, index, bindGroupId);
	}

	/// <summary>
	/// Sets a vertex buffer for a render pass.
	/// </summary>
	public async Task SetVertexBufferAsync(int passEncoderId, int slot, int bufferId)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.SetVertexBufferAsync(passEncoderId, slot, bufferId);
	}

	/// <summary>
	/// Sets an index buffer for a render pass.
	/// </summary>
	public async Task SetIndexBufferAsync(int passEncoderId, int bufferId, string format = "uint16")
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.SetIndexBufferAsync(passEncoderId, bufferId, format);
	}

	/// <summary>
	/// Draws vertices.
	/// </summary>
	public async Task DrawAsync(int passEncoderId, int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.DrawAsync(passEncoderId, vertexCount, instanceCount, firstVertex, firstInstance);
	}

	/// <summary>
	/// Draws indexed vertices.
	/// </summary>
	public async Task DrawIndexedAsync(int passEncoderId, int indexCount, int instanceCount = 1, int firstIndex = 0, int baseVertex = 0, int firstInstance = 0)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.DrawIndexedAsync(passEncoderId, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
	}

	/// <summary>
	/// Ends a render pass.
	/// </summary>
	public async Task EndRenderPassAsync(int passEncoderId)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		await _interop.EndRenderPassAsync(passEncoderId);
	}

	/// <summary>
	/// Finishes recording and returns the command buffer resource ID.
	/// </summary>
	/// <returns>The command buffer resource ID.</returns>
	public async Task<int> FinishAsync()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		return await _interop.FinishCommandEncoderAsync(_resourceId);
	}

	/// <summary>
	/// Finishes recording and returns the command buffer resource ID synchronously.
	/// </summary>
	/// <returns>The command buffer resource ID.</returns>
	[Obsolete("Use FinishAsync instead")]
	public int Finish()
	{
		return FinishAsync().GetAwaiter().GetResult();
	}

	private static object ConvertRenderPassDescriptor(RenderPassDescriptor descriptor)
	{
		var colorAttachments = descriptor.ColorAttachments?.Select(att => new
		{
			viewId = att.ViewId,
			resolveTargetId = att.ResolveTargetId,
			loadOp = att.LoadOp,
			storeOp = att.StoreOp,
			clearValue = att.ClearValue != null ? new
			{
				r = att.ClearValue.R,
				g = att.ClearValue.G,
				b = att.ClearValue.B,
				a = att.ClearValue.A
			} : null
		}).ToArray();

		object? depthStencilAttachment = null;
		if (descriptor.DepthStencilAttachment != null)
		{
			var ds = descriptor.DepthStencilAttachment;
			depthStencilAttachment = new
			{
				viewId = ds.ViewId,
				depthLoadOp = ds.DepthLoadOp,
				depthStoreOp = ds.DepthStoreOp,
				depthClearValue = ds.DepthClearValue,
				stencilLoadOp = ds.StencilLoadOp,
				stencilStoreOp = ds.StencilStoreOp,
				stencilClearValue = ds.StencilClearValue
			};
		}

		return new
		{
			label = descriptor.Label,
			colorAttachments,
			depthStencilAttachment
		};
	}

	/// <summary>
	/// Disposes the command encoder synchronously.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		DisposeAsync().AsTask().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Disposes the command encoder asynchronously.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		try
		{
			await _service.ReleaseResourceAsync(_resourceId);
		}
		catch
		{
			// Ignore errors during disposal
		}

		GC.SuppressFinalize(this);
	}
}
