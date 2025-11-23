namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Represents a WebGPU command encoder for recording GPU commands.
/// </summary>
public class PDWebGpuCommandEncoder : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuCommandEncoder"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	internal PDWebGpuCommandEncoder(Services.IPDWebGpuService service, int resourceId)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
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
	/// Finishes recording and returns the command buffer resource ID.
	/// </summary>
	/// <returns>The command buffer resource ID.</returns>
	public int Finish()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuCommandEncoder));
		}

		// The finish operation would be called through JavaScript interop
		// For now, return the resource ID which represents the finished command buffer
		return _resourceId;
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
