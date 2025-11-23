namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Specifies the texture filtering mode.
/// </summary>
public enum FilterMode
{
	/// <summary>
	/// Nearest neighbor filtering.
	/// </summary>
	Nearest,

	/// <summary>
	/// Linear filtering.
	/// </summary>
	Linear
}

/// <summary>
/// Specifies the texture address mode.
/// </summary>
public enum AddressMode
{
	/// <summary>
	/// Clamp to edge.
	/// </summary>
	ClampToEdge,

	/// <summary>
	/// Repeat the texture.
	/// </summary>
	Repeat,

	/// <summary>
	/// Mirror repeat.
	/// </summary>
	MirrorRepeat
}

/// <summary>
/// Represents a WebGPU texture sampler.
/// </summary>
public class PDWebGpuSampler : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuSampler"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	/// <param name="magFilter">The magnification filter mode.</param>
	/// <param name="minFilter">The minification filter mode.</param>
	/// <param name="addressModeU">The U address mode.</param>
	/// <param name="addressModeV">The V address mode.</param>
	internal PDWebGpuSampler(
		Services.IPDWebGpuService service,
		int resourceId,
		FilterMode magFilter,
		FilterMode minFilter,
		AddressMode addressModeU,
		AddressMode addressModeV)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
		MagFilter = magFilter;
		MinFilter = minFilter;
		AddressModeU = addressModeU;
		AddressModeV = addressModeV;
	}

	/// <summary>
	/// Gets the magnification filter mode.
	/// </summary>
	public FilterMode MagFilter { get; }

	/// <summary>
	/// Gets the minification filter mode.
	/// </summary>
	public FilterMode MinFilter { get; }

	/// <summary>
	/// Gets the U address mode.
	/// </summary>
	public AddressMode AddressModeU { get; }

	/// <summary>
	/// Gets the V address mode.
	/// </summary>
	public AddressMode AddressModeV { get; }

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the sampler has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Disposes the sampler synchronously.
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
	/// Disposes the sampler asynchronously.
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
