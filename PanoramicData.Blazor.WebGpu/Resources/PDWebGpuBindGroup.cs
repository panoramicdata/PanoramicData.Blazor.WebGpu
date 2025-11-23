namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Represents a WebGPU bind group that binds resources to a pipeline.
/// </summary>
public class PDWebGpuBindGroup : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuBindGroup"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	internal PDWebGpuBindGroup(Services.IPDWebGpuService service, int resourceId)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
	}

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the bind group has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Disposes the bind group synchronously.
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
	/// Disposes the bind group asynchronously.
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
