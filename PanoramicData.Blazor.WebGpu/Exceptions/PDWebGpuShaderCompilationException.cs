namespace PanoramicData.Blazor.WebGpu;

/// <summary>
/// Exception thrown when WGSL shader compilation fails.
/// </summary>
public class PDWebGpuShaderCompilationException : PDWebGpuException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuShaderCompilationException"/> class.
	/// </summary>
	public PDWebGpuShaderCompilationException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuShaderCompilationException"/> class 
	/// with a specified error message.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public PDWebGpuShaderCompilationException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuShaderCompilationException"/> class 
	/// with shader source and error details.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="shaderSource">The WGSL shader source code that failed to compile.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public PDWebGpuShaderCompilationException(string message, string shaderSource, Exception innerException)
		: base(message, innerException)
	{
		ShaderSource = shaderSource;
		ParseCompilationErrors(innerException.Message);
	}

	/// <summary>
	/// Gets the WGSL shader source code that failed to compile.
	/// </summary>
	public string? ShaderSource { get; }

	/// <summary>
	/// Gets the line number where the error occurred (if available).
	/// </summary>
	public int? LineNumber { get; private set; }

	/// <summary>
	/// Gets the detailed compilation error message.
	/// </summary>
	public string? CompilationError { get; private set; }

	private void ParseCompilationErrors(string errorMessage)
	{
		// Try to extract line number from error message
		// Format is typically "Line X: error message"
		var lines = errorMessage.Split('\n');
		foreach (var line in lines)
		{
			if (line.StartsWith("Line ", StringComparison.OrdinalIgnoreCase))
			{
				var parts = line.Split(':');
				if (parts.Length >= 2)
				{
					var lineNumPart = parts[0].Replace("Line", "").Trim();
					if (int.TryParse(lineNumPart, out var lineNum))
					{
						LineNumber = lineNum;
						CompilationError = string.Join(":", parts.Skip(1)).Trim();
						break;
					}
				}
			}
		}

		// If no line number was found, store the full error
		if (!LineNumber.HasValue)
		{
			CompilationError = errorMessage;
		}
	}

	/// <summary>
	/// Gets a formatted error message including line number and shader context.
	/// </summary>
	/// <returns>Formatted error message.</returns>
	public string GetFormattedError()
	{
		if (!LineNumber.HasValue || string.IsNullOrEmpty(ShaderSource))
		{
			return Message;
		}

		var lines = ShaderSource.Split('\n');
		var errorContext = new System.Text.StringBuilder();
		errorContext.AppendLine($"Shader compilation failed at line {LineNumber}:");
		errorContext.AppendLine(CompilationError);
		errorContext.AppendLine();
		errorContext.AppendLine("Context:");

		// Show a few lines before and after the error
		var startLine = Math.Max(0, LineNumber.Value - 3);
		var endLine = Math.Min(lines.Length - 1, LineNumber.Value + 2);

		for (var i = startLine; i <= endLine; i++)
		{
			var marker = i == LineNumber.Value - 1 ? ">>> " : "    ";
			errorContext.AppendLine($"{marker}{i + 1,4}: {lines[i]}");
		}

		return errorContext.ToString();
	}
}
