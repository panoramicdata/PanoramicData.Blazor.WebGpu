using PanoramicData.Blazor.WebGpu.Interop;

namespace PanoramicData.Blazor.WebGpu.Diagnostics;

/// <summary>
/// Provides diagnostic information and user-friendly error messages for WebGPU issues.
/// </summary>
public static class DiagnosticHelper
{
	/// <summary>
	/// Gets a user-friendly error message for WebGPU not being supported.
	/// </summary>
	/// <param name="compatibilityInfo">Browser compatibility information.</param>
	/// <returns>Formatted error message with suggestions.</returns>
	public static string GetNotSupportedMessage(WebGpuCompatibilityInfo compatibilityInfo)
	{
		var message = new System.Text.StringBuilder();

		message.AppendLine("⚠️ WebGPU Not Available");
		message.AppendLine();
		message.AppendLine($"Your current browser ({compatibilityInfo.BrowserName} {compatibilityInfo.BrowserVersion}) does not support WebGPU.");
		message.AppendLine();
		message.AppendLine("Supported Browsers:");
		message.AppendLine("  ✓ Google Chrome 113 or later");
		message.AppendLine("  ✓ Microsoft Edge 113 or later");
		message.AppendLine("  ✓ Opera 99 or later");
		message.AppendLine();

		if (compatibilityInfo.SupportsWithFlags)
		{
			message.AppendLine("Your browser may support WebGPU with additional configuration:");
			message.AppendLine();

			if (compatibilityInfo.BrowserName?.Contains("Firefox", StringComparison.OrdinalIgnoreCase) == true)
			{
				message.AppendLine("Firefox Setup:");
				message.AppendLine("  1. Type 'about:config' in the address bar");
				message.AppendLine("  2. Search for 'dom.webgpu.enabled'");
				message.AppendLine("  3. Set it to 'true'");
				message.AppendLine("  4. Restart Firefox");
			}
			else if (compatibilityInfo.BrowserName?.Contains("Safari", StringComparison.OrdinalIgnoreCase) == true)
			{
				message.AppendLine("Safari Setup:");
				message.AppendLine("  1. Download Safari Technology Preview");
				message.AppendLine("  2. Open Develop menu");
				message.AppendLine("  3. Enable 'WebGPU' in Experimental Features");
			}
		}
		else
		{
			message.AppendLine("We recommend using Google Chrome or Microsoft Edge for the best WebGPU experience.");
		}

		return message.ToString();
	}

	/// <summary>
	/// Gets a user-friendly error message for device initialization failures.
	/// </summary>
	/// <param name="exception">The exception that occurred.</param>
	/// <returns>Formatted error message with troubleshooting steps.</returns>
	public static string GetDeviceErrorMessage(Exception exception)
	{
		var message = new System.Text.StringBuilder();

		message.AppendLine("⚠️ WebGPU Device Error");
		message.AppendLine();
		message.AppendLine($"Failed to initialize WebGPU: {exception.Message}");
		message.AppendLine();
		message.AppendLine("Troubleshooting Steps:");
		message.AppendLine("  1. Refresh the page (F5 or Ctrl+R)");
		message.AppendLine("  2. Restart your browser");
		message.AppendLine("  3. Update your GPU drivers");
		message.AppendLine("  4. Check if hardware acceleration is enabled in browser settings");
		message.AppendLine("  5. Try disabling browser extensions");
		message.AppendLine();
		message.AppendLine("If the problem persists, your GPU may not support WebGPU features.");

		return message.ToString();
	}

	/// <summary>
	/// Gets a user-friendly error message for shader compilation failures.
	/// </summary>
	/// <param name="exception">The shader compilation exception.</param>
	/// <returns>Formatted error message.</returns>
	public static string GetShaderErrorMessage(PDWebGpuShaderCompilationException exception)
	{
		var message = new System.Text.StringBuilder();

		message.AppendLine("⚠️ Shader Compilation Error");
		message.AppendLine();

		if (exception.LineNumber.HasValue)
		{
			message.AppendLine($"Error at line {exception.LineNumber}:");
		}

		message.AppendLine(exception.CompilationError ?? exception.Message);
		message.AppendLine();

		if (!string.IsNullOrEmpty(exception.ShaderSource))
		{
			var lines = exception.ShaderSource.Split('\n');
			var startLine = Math.Max(0, (exception.LineNumber ?? 1) - 3);
			var endLine = Math.Min(lines.Length - 1, (exception.LineNumber ?? 1) + 2);

			message.AppendLine("Shader Context:");
			for (var i = startLine; i <= endLine; i++)
			{
				var marker = i == (exception.LineNumber ?? 1) - 1 ? ">>> " : "    ";
				message.AppendLine($"{marker}{i + 1,4}: {lines[i]}");
			}
		}

		return message.ToString();
	}

	/// <summary>
	/// Formats a number with appropriate units (K, M, B).
	/// </summary>
	/// <param name="value">The value to format.</param>
	/// <returns>Formatted string.</returns>
	public static string FormatNumber(long value)
	{
		if (value >= 1_000_000_000)
		{
			return $"{value / 1_000_000_000.0:F1}B";
		}
		else if (value >= 1_000_000)
		{
			return $"{value / 1_000_000.0:F1}M";
		}
		else if (value >= 1_000)
		{
			return $"{value / 1_000.0:F1}K";
		}
		return value.ToString();
	}

	/// <summary>
	/// Gets browser capability information as a formatted string.
	/// </summary>
	/// <param name="compatibilityInfo">Browser compatibility information.</param>
	/// <returns>Formatted capability information.</returns>
	public static string GetBrowserCapabilities(WebGpuCompatibilityInfo compatibilityInfo)
	{
		var info = new System.Text.StringBuilder();

		info.AppendLine("Browser Information:");
		info.AppendLine($"  Name: {compatibilityInfo.BrowserName ?? "Unknown"}");
		info.AppendLine($"  Version: {compatibilityInfo.BrowserVersion ?? "Unknown"}");
		info.AppendLine($"  Platform: {compatibilityInfo.Platform}");
		info.AppendLine($"  Vendor: {compatibilityInfo.Vendor}");
		info.AppendLine($"  WebGPU Support: {(compatibilityInfo.IsSupported ? "✓ Yes" : "✗ No")}");

		if (compatibilityInfo.SupportsWithFlags && !compatibilityInfo.IsSupported)
		{
			info.AppendLine($"  Supports with Flags: ✓ Yes (configuration required)");
		}

		return info.ToString();
	}
}
