namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Represents shader compilation information including errors and warnings.
/// </summary>
public class ShaderCompilationInfo
{
	/// <summary>
	/// Gets or sets whether the shader compiled successfully.
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// Gets or sets the compilation error message, if any.
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Gets or sets the line number where the error occurred, if applicable.
	/// </summary>
	public int? ErrorLine { get; set; }

	/// <summary>
	/// Gets or sets the column number where the error occurred, if applicable.
	/// </summary>
	public int? ErrorColumn { get; set; }

	/// <summary>
	/// Gets or sets any compilation warnings.
	/// </summary>
	public List<string> Warnings { get; set; } = [];
}

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
	/// <param name="name">Optional name for debugging purposes.</param>
	/// <param name="compilationInfo">Optional compilation information.</param>
	internal PDWebGpuShader(Services.IPDWebGpuService service, int resourceId, string wgslCode, string? name = null, ShaderCompilationInfo? compilationInfo = null)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
		_resourceId = resourceId;
		WgslCode = wgslCode ?? throw new ArgumentNullException(nameof(wgslCode));
		Name = name;
		CompilationInfo = compilationInfo ?? new ShaderCompilationInfo { Success = true };
	}

	/// <summary>
	/// Gets the WGSL shader source code.
	/// </summary>
	public string WgslCode { get; }

	/// <summary>
	/// Gets the optional shader name for debugging.
	/// </summary>
	public string? Name { get; }

	/// <summary>
	/// Gets the compilation information.
	/// </summary>
	public ShaderCompilationInfo CompilationInfo { get; }

	/// <summary>
	/// Gets the resource ID.
	/// </summary>
	public int ResourceId => _resourceId;

	/// <summary>
	/// Gets whether the shader has been disposed.
	/// </summary>
	public bool IsDisposed => _disposed;

	/// <summary>
	/// Validates the WGSL shader source code syntax.
	/// </summary>
	/// <returns>Validation result with any errors or warnings.</returns>
	public static ShaderCompilationInfo Validate(string wgslCode)
	{
		if (string.IsNullOrWhiteSpace(wgslCode))
		{
			return new ShaderCompilationInfo
			{
				Success = false,
				ErrorMessage = "Shader code cannot be empty",
				ErrorLine = 1,
				ErrorColumn = 1
			};
		}

		var info = new ShaderCompilationInfo { Success = true };

		// Basic syntax validation
		var lines = wgslCode.Split('\n');
		for (var i = 0; i < lines.Length; i++)
		{
			var line = lines[i].Trim();
			
			// Check for common syntax errors
			if (line.Contains("@vertex") && !line.StartsWith("@vertex"))
			{
				info.Warnings.Add($"Line {i + 1}: @vertex attribute should be at the start of the line");
			}
			if (line.Contains("@fragment") && !line.StartsWith("@fragment"))
			{
				info.Warnings.Add($"Line {i + 1}: @fragment attribute should be at the start of the line");
			}
		}

		return info;
	}

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
