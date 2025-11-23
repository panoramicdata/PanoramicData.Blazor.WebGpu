namespace PanoramicData.Blazor.WebGpu.Interop;

/// <summary>
/// Contains browser compatibility information for WebGPU support detection.
/// </summary>
public class WebGpuCompatibilityInfo
{
	/// <summary>
	/// Gets or sets whether WebGPU is supported.
	/// </summary>
	public bool IsSupported { get; set; }

	/// <summary>
	/// Gets or sets the browser user agent string.
	/// </summary>
	public string UserAgent { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the browser vendor.
	/// </summary>
	public string Vendor { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the platform/OS.
	/// </summary>
	public string Platform { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the error message if WebGPU is not supported.
	/// </summary>
	public string? ErrorMessage { get; set; }

	/// <summary>
	/// Gets or sets the detected browser name.
	/// </summary>
	public string? BrowserName { get; set; }

	/// <summary>
	/// Gets or sets the detected browser version.
	/// </summary>
	public string? BrowserVersion { get; set; }

	/// <summary>
	/// Gets or sets whether the browser is known to support WebGPU with flags enabled.
	/// </summary>
	public bool SupportsWithFlags { get; set; }
}
