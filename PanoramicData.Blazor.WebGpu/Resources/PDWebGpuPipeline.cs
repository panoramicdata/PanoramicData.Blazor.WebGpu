namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Specifies the pipeline type.
/// </summary>
public enum PipelineType
{
	/// <summary>
	/// Render pipeline.
	/// </summary>
	Render,

	/// <summary>
	/// Compute pipeline.
	/// </summary>
	Compute
}

/// <summary>
/// Represents a WebGPU pipeline (render or compute).
/// </summary>
public class PDWebGpuPipeline : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuPipeline"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	/// <param name="pipelineType">The type of pipeline.</param>
	internal PDWebGpuPipeline(Services.IPDWebGpuService service, int resourceId, PipelineType pipelineType)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
		PipelineType = pipelineType;
	}

	/// <summary>
	/// Gets the pipeline type.
	/// </summary>
	public PipelineType PipelineType { get; }

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the pipeline has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Disposes the pipeline synchronously.
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
	/// Disposes the pipeline asynchronously.
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
