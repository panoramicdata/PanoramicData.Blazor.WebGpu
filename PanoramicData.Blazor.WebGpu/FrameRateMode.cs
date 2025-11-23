namespace PanoramicData.Blazor.WebGpu;

/// <summary>
/// Specifies the frame rate mode for rendering.
/// </summary>
public enum FrameRateMode
{
	/// <summary>
	/// Render as fast as possible (VSync limited).
	/// </summary>
	Variable,

	/// <summary>
	/// Render at a fixed target frame rate.
	/// </summary>
	Fixed
}
