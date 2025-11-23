namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Specifies the type of buffer.
/// </summary>
public enum BufferType
{
	/// <summary>
	/// Vertex buffer containing vertex data.
	/// </summary>
	Vertex,

	/// <summary>
	/// Index buffer containing index data.
	/// </summary>
	Index,

	/// <summary>
	/// Uniform buffer containing shader uniform data.
	/// </summary>
	Uniform,

	/// <summary>
	/// Storage buffer for general-purpose data storage.
	/// </summary>
	Storage
}

/// <summary>
/// Represents a WebGPU buffer resource.
/// </summary>
public class PDWebGpuBuffer : IAsyncDisposable, IDisposable
{
	private readonly Services.IPDWebGpuService _service;
	private int _resourceId;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuBuffer"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	/// <param name="resourceId">The resource ID from JavaScript.</param>
	/// <param name="size">The size of the buffer in bytes.</param>
	/// <param name="bufferType">The type of buffer.</param>
	/// <param name="name">Optional name for debugging purposes.</param>
	internal PDWebGpuBuffer(Services.IPDWebGpuService service, int resourceId, long size, BufferType bufferType, string? name = null)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
		Size = size;
		BufferType = bufferType;
		Name = name;
	}

	/// <summary>
	/// Gets the type of buffer.
	/// </summary>
	public BufferType BufferType { get; }

	/// <summary>
	/// Gets the size of the buffer in bytes.
	/// </summary>
	public long Size { get; }

	/// <summary>
	/// Gets the optional buffer name for debugging.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the buffer has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Updates the buffer data.
	/// </summary>
	/// <param name="data">The new buffer data.</param>
	/// <param name="offset">Optional offset in bytes.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task UpdateAsync(byte[] data, long offset = 0)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(PDWebGpuBuffer));
		}

		if (data == null || data.Length == 0)
		{
			throw new ArgumentException("Buffer data cannot be null or empty", nameof(data));
		}

		if (offset < 0 || offset + data.Length > Size)
		{
			throw new ArgumentOutOfRangeException(nameof(offset), "Offset and data length exceed buffer size");
		}

		// TODO: Implement JavaScript interop for buffer update
		await Task.CompletedTask; // Placeholder
	}

	/// <summary>
	/// Disposes the buffer synchronously.
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
	/// Disposes the buffer asynchronously.
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
