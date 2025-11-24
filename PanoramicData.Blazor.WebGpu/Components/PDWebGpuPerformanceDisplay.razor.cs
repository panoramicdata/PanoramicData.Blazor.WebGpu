using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor.WebGpu.Performance;

namespace PanoramicData.Blazor.WebGpu.Components;

/// <summary>
/// Component that displays real-time performance metrics overlay.
/// </summary>
public partial class PDWebGpuPerformanceDisplay : PDWebGpuComponentBase
{
	private readonly PerformanceMetrics _metrics = new();
	private System.Threading.Timer? _updateTimer;
	private double _targetFrameTime;

	[CascadingParameter]
	private PDWebGpuContainer? Container { get; set; }

	/// <summary>
	/// Gets or sets the performance display options.
	/// </summary>
	[Parameter]
	public PDWebGpuPerformanceDisplayOptions Options { get; set; } = new();

	/// <summary>
	/// Gets or sets the target frame rate (used for frame time usage calculation).
	/// </summary>
	[Parameter]
	public int TargetFrameRate { get; set; } = 60;

	/// <summary>
	/// Gets the performance metrics.
	/// </summary>
	public PerformanceMetrics Metrics => _metrics;

	/// <inheritdoc/>
	protected override void OnInitialized()
	{
		base.OnInitialized();

		// Register with container for automatic updates
		Container?.RegisterPerformanceDisplay(this);

		// Calculate target frame time
		_targetFrameTime = TargetFrameRate > 0 ? 1000.0 / TargetFrameRate : 0;

		// Set up periodic UI refresh
		_updateTimer = new System.Threading.Timer(
			_ => InvokeAsync(StateHasChanged),
			null,
			Options.UpdateIntervalMs,
			Options.UpdateIntervalMs);
	}

	/// <inheritdoc/>
	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		// Recalculate target frame time when parameters change
		_targetFrameTime = TargetFrameRate > 0 ? 1000.0 / TargetFrameRate : 0;
	}

	/// <summary>
	/// Records a frame for performance tracking.
	/// </summary>
	/// <param name="frameTime">Frame time in milliseconds.</param>
	public void RecordFrame(double frameTime)
	{
		_metrics.RecordFrame(frameTime);
	}

	/// <summary>
	/// Sets the number of draw calls for the current frame.
	/// </summary>
	/// <param name="drawCalls">Number of draw calls.</param>
	public void SetDrawCalls(int drawCalls)
	{
		_metrics.DrawCalls = drawCalls;
	}

	/// <summary>
	/// Sets the number of triangles rendered in the current frame.
	/// </summary>
	/// <param name="triangleCount">Number of triangles.</param>
	public void SetTriangleCount(long triangleCount)
	{
		_metrics.TriangleCount = triangleCount;
	}

	/// <summary>
	/// Resets all performance metrics.
	/// </summary>
	public void Reset()
	{
		_metrics.Reset();
	}

	private string GetContainerStyle()
	{
		return $"--bg-opacity: {Options.BackgroundOpacity};";
	}

	private string GetPositionClass()
	{
		return Options.Position switch
		{
			CornerPosition.TopLeft => "top-left",
			CornerPosition.TopRight => "top-right",
			CornerPosition.BottomLeft => "bottom-left",
			CornerPosition.BottomRight => "bottom-right",
			_ => "top-right"
		};
	}

	private string GetUsageClass()
	{
		var usage = _metrics.GetFrameTimeUsage(_targetFrameTime);
		if (usage >= 90)
		{
			return "critical";
		}
		else if (usage >= 70)
		{
			return "warning";
		}
		return "";
	}

	private static string FormatNumber(long number)
	{
		if (number >= 1_000_000)
		{
			return $"{number / 1_000_000.0:F1}M";
		}
		else if (number >= 1_000)
		{
			return $"{number / 1_000.0:F1}K";
		}
		return number.ToString();
	}

	/// <inheritdoc/>
	protected override async ValueTask DisposeAsyncCore()
	{
		// Unregister from container
		Container?.UnregisterPerformanceDisplay();

		_updateTimer?.Dispose();
		await base.DisposeAsyncCore();
	}
}
