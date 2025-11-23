using PanoramicData.Blazor.WebGpu.Resources;
using PanoramicData.Blazor.WebGpu.Services;

namespace PanoramicData.Blazor.WebGpu.Utilities;

/// <summary>
/// Utility class for loading and managing WGSL shaders with hot-reload support.
/// </summary>
public class ShaderLoader
{
	private readonly IPDWebGpuService _service;
	private readonly Dictionary<string, PDWebGpuShader> _loadedShaders = [];
	private readonly Dictionary<string, string> _shaderSources = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="ShaderLoader"/> class.
	/// </summary>
	/// <param name="service">The WebGPU service.</param>
	public ShaderLoader(IPDWebGpuService service)
	{
		_service = service ?? throw new ArgumentNullException(nameof(service));
	}

	/// <summary>
	/// Event raised when a shader is reloaded.
	/// </summary>
	public event EventHandler<ShaderReloadedEventArgs>? ShaderReloaded;

	/// <summary>
	/// Loads a WGSL shader from source code.
	/// </summary>
	/// <param name="name">Unique name for the shader.</param>
	/// <param name="wgslCode">The WGSL source code.</param>
	/// <param name="validate">Whether to validate the shader before loading.</param>
	/// <returns>The loaded shader.</returns>
	/// <exception cref="PDWebGpuShaderCompilationException">Thrown when shader compilation fails.</exception>
	public async Task<PDWebGpuShader> LoadShaderAsync(string name, string wgslCode, bool validate = true)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Shader name cannot be empty", nameof(name));
		}

		// Validate shader if requested
		if (validate)
		{
			var validationInfo = PDWebGpuShader.Validate(wgslCode);
			if (!validationInfo.Success)
			{
				var errorMsg = validationInfo.ErrorMessage ?? "Shader validation failed";
				if (validationInfo.ErrorLine.HasValue)
				{
					errorMsg = $"Line {validationInfo.ErrorLine}: {errorMsg}";
				}
				throw new PDWebGpuShaderCompilationException(errorMsg);
			}
		}

		// Dispose old shader if exists
		if (_loadedShaders.TryGetValue(name, out var oldShader))
		{
			await oldShader.DisposeAsync();
			_loadedShaders.Remove(name);
		}

		// Create new shader
		var shader = await _service.CreateShaderAsync(wgslCode);
		_loadedShaders[name] = shader;
		_shaderSources[name] = wgslCode;

		return shader;
	}

	/// <summary>
	/// Reloads a previously loaded shader with new source code.
	/// </summary>
	/// <param name="name">The name of the shader to reload.</param>
	/// <param name="wgslCode">The new WGSL source code.</param>
	/// <param name="validate">Whether to validate the shader before reloading.</param>
	/// <returns>The reloaded shader.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the shader hasn't been loaded yet.</exception>
	/// <exception cref="PDWebGpuShaderCompilationException">Thrown when shader compilation fails.</exception>
	public async Task<PDWebGpuShader> ReloadShaderAsync(string name, string wgslCode, bool validate = true)
	{
		if (!_loadedShaders.ContainsKey(name))
		{
			throw new InvalidOperationException($"Shader '{name}' has not been loaded yet. Use LoadShaderAsync first.");
		}

		var shader = await LoadShaderAsync(name, wgslCode, validate);
		
		// Raise reload event
		ShaderReloaded?.Invoke(this, new ShaderReloadedEventArgs(name, shader));

		return shader;
	}

	/// <summary>
	/// Gets a previously loaded shader by name.
	/// </summary>
	/// <param name="name">The shader name.</param>
	/// <returns>The shader, or null if not found.</returns>
	public PDWebGpuShader? GetShader(string name)
	{
		_loadedShaders.TryGetValue(name, out var shader);
		return shader;
	}

	/// <summary>
	/// Gets the source code of a previously loaded shader.
	/// </summary>
	/// <param name="name">The shader name.</param>
	/// <returns>The shader source code, or null if not found.</returns>
	public string? GetShaderSource(string name)
	{
		_shaderSources.TryGetValue(name, out var source);
		return source;
	}

	/// <summary>
	/// Checks if a shader with the given name has been loaded.
	/// </summary>
	/// <param name="name">The shader name.</param>
	/// <returns>True if the shader is loaded; otherwise, false.</returns>
	public bool IsLoaded(string name)
	{
		return _loadedShaders.ContainsKey(name);
	}

	/// <summary>
	/// Gets all loaded shader names.
	/// </summary>
	/// <returns>Collection of shader names.</returns>
	public IEnumerable<string> GetLoadedShaderNames()
	{
		return _loadedShaders.Keys.ToList();
	}

	/// <summary>
	/// Disposes all loaded shaders.
	/// </summary>
	public async ValueTask DisposeAllAsync()
	{
		foreach (var shader in _loadedShaders.Values)
		{
			await shader.DisposeAsync();
		}

		_loadedShaders.Clear();
		_shaderSources.Clear();
	}
}

/// <summary>
/// Event args for shader reload events.
/// </summary>
public class ShaderReloadedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ShaderReloadedEventArgs"/> class.
	/// </summary>
	/// <param name="shaderName">The name of the reloaded shader.</param>
	/// <param name="shader">The reloaded shader instance.</param>
	public ShaderReloadedEventArgs(string shaderName, PDWebGpuShader shader)
	{
		ShaderName = shaderName;
		Shader = shader;
	}

	/// <summary>
	/// Gets the name of the reloaded shader.
	/// </summary>
	public string ShaderName { get; }

	/// <summary>
	/// Gets the reloaded shader instance.
	/// </summary>
	public PDWebGpuShader Shader { get; }
}
