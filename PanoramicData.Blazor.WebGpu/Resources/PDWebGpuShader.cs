namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Represents a WGSL shader module.
/// </summary>
public class PDWebGpuShader : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuShader"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	/// <param name="wgslCode">The WGSL shader source code.</param>
	internal PDWebGpuShader(Services.IPDWebGpuService service, int resourceId, string wgslCode)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
		WgslCode = wgslCode ?? throw new ArgumentNullException(nameof(wgslCode));
	}

	/// <summary>
	/// Gets the WGSL shader source code.
	/// </summary>
	public string WgslCode { get; }

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the shader has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Disposes the shader synchronously.
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
	/// Disposes the shader asynchronously.
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
