namespace PanoramicData.Blazor.WebGpu.Performance;

/// <summary>
/// Specifies the position of the performance display overlay.
/// </summary>
public enum CornerPosition
{
	/// <summary>
	/// Top-left corner.
	/// </summary>
	TopLeft,

	/// <summary>
	/// Top-right corner.
	/// </summary>
	TopRight,

	/// <summary>
	/// Bottom-left corner.
	/// </summary>
	BottomLeft,

	/// <summary>
	/// Bottom-right corner.
	/// </summary>
	BottomRight
}

/// <summary>
/// Configuration options for the performance display component.
/// </summary>
public class PDWebGpuPerformanceDisplayOptions
{
	/// <summary>
	/// Gets or sets whether to display frames per second.
	/// </summary>
	public bool ShowFPS { get; set; } = true;

	/// <summary>
	/// Gets or sets whether to display frame time in milliseconds.
	/// </summary>
	public bool ShowFrameTime { get; set; } = true;

	/// <summary>
	/// Gets or sets whether to display frame time usage percentage (for fixed frame rate mode).
	/// </summary>
	public bool ShowFrameTimeUsage { get; set; }

	/// <summary>
	/// Gets or sets whether to display draw call count.
	/// </summary>
	public bool ShowDrawCalls { get; set; }

	/// <summary>
	/// Gets or sets whether to display triangle count.
	/// </summary>
	public bool ShowTriangleCount { get; set; }

	/// <summary>
	/// Gets or sets the position of the performance display.
	/// </summary>
	public CornerPosition Position { get; set; } = CornerPosition.TopRight;

	/// <summary>
	/// Gets or sets the background opacity (0.0 - 1.0).
	/// </summary>
	public double BackgroundOpacity { get; set; } = 0.7;

	/// <summary>
	/// Gets or sets the update interval in milliseconds (how often to refresh the display).
	/// </summary>
	public int UpdateIntervalMs { get; set; } = 500;

	/// <summary>
	/// Gets or sets custom user-defined metrics to display.
	/// </summary>
	public Dictionary<string, Func<string>> CustomMetrics { get; set; } = [];
}
