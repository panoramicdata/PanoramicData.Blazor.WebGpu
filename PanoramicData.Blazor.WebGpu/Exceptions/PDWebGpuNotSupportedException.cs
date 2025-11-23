namespace PanoramicData.Blazor.WebGpu;

/// <summary>
/// Exception thrown when WebGPU is not supported in the current browser.
/// </summary>
public class PDWebGpuNotSupportedException : PDWebGpuException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuNotSupportedException"/> class.
	/// </summary>
	public PDWebGpuNotSupportedException()
		: base("WebGPU is not supported in this browser")
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuNotSupportedException"/> class 
	/// with browser compatibility information.
	/// </summary>
	/// <param name="compatibilityInfo">Browser compatibility information.</param>
	public PDWebGpuNotSupportedException(Interop.WebGpuCompatibilityInfo compatibilityInfo)
		: base(compatibilityInfo.ErrorMessage ?? "WebGPU is not supported in this browser")
	{
		CompatibilityInfo = compatibilityInfo;
	}

	/// <summary>
	/// Gets the browser compatibility information.
	/// </summary>
	public Interop.WebGpuCompatibilityInfo? CompatibilityInfo { get; }

	/// <summary>
	/// Gets suggested actions for enabling WebGPU support.
	/// </summary>
	public static string Suggestion => "Please use one of the following browsers with WebGPU enabled:\n" +
		"- Chrome 113 or later\n" +
		"- Edge 113 or later\n" +
		"- Opera 99 or later\n\n" +
		"For Firefox and Safari, WebGPU may need to be enabled in experimental features.";

	/// <summary>
	/// Gets a formatted error message with browser details and suggestions.
	/// </summary>
	/// <returns>Formatted error message.</returns>
	public string GetDetailedMessage()
	{
		var details = new System.Text.StringBuilder();
		details.AppendLine(Message);
		details.AppendLine();

		if (CompatibilityInfo != null)
		{
			details.AppendLine("Browser Information:");
			details.AppendLine($"  User Agent: {CompatibilityInfo.UserAgent}");
			details.AppendLine($"  Vendor: {CompatibilityInfo.Vendor}");
			details.AppendLine($"  Platform: {CompatibilityInfo.Platform}");
			details.AppendLine();
		}

		details.AppendLine(Suggestion);

		return details.ToString();
	}
}
