namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Specifies the texture format.
/// </summary>
public enum TextureFormat
{
	/// <summary>
	/// RGBA 8-bit unsigned normalized.
	/// </summary>
	RGBA8Unorm,

	/// <summary>
	/// BGRA 8-bit unsigned normalized.
	/// </summary>
	BGRA8Unorm,

	/// <summary>
	/// Depth 24-bit plus stencil 8-bit.
	/// </summary>
	Depth24PlusStencil8,

	/// <summary>
	/// Depth 32-bit float.
	/// </summary>
	Depth32Float
}

/// <summary>
/// Represents a WebGPU texture resource.
/// </summary>
public class PDWebGpuTexture : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuTexture"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	/// <param name="width">The texture width in pixels.</param>
	/// <param name="height">The texture height in pixels.</param>
	/// <param name="format">The texture format.</param>
	internal PDWebGpuTexture(Services.IPDWebGpuService service, int resourceId, int width, int height, TextureFormat format)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
		Width = width;
		Height = height;
		Format = format;
	}

	/// <summary>
	/// Gets the texture width in pixels.
	/// </summary>
	public int Width { get; }

	/// <summary>
	/// Gets the texture height in pixels.
	/// </summary>
	public int Height { get; }

	/// <summary>
	/// Gets the texture format.
	/// </summary>
	public TextureFormat Format { get; }

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the texture has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Disposes the texture synchronously.
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
	/// Disposes the texture asynchronously.
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
