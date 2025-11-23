namespace PanoramicData.Blazor.WebGpu.Performance;

/// <summary>
/// Tracks and calculates performance metrics for WebGPU rendering.
/// </summary>
public class PerformanceMetrics
{
	private readonly Queue<double> _frameTimes = new();
	private readonly int _maxSamples = 60; // Track last 60 frames
	private double _lastUpdateTime;
	private int _frameCount;
	private double _fps;
	private double _averageFrameTime;

	/// <summary>
	/// Gets the current frames per second.
	/// </summary>
	public double FPS => _fps;

	/// <summary>
	/// Gets the average frame time in milliseconds.
	/// </summary>
	public double AverageFrameTime => _averageFrameTime;

	/// <summary>
	/// Gets or sets the number of draw calls in the last frame.
	/// </summary>
	public int DrawCalls { get; set; }

	/// <summary>
	/// Gets or sets the number of triangles rendered in the last frame.
	/// </summary>
	public long TriangleCount { get; set; }

	/// <summary>
	/// Gets the frame time usage percentage (for fixed frame rate mode).
	/// </summary>
	/// <param name="targetFrameTime">Target frame time in milliseconds.</param>
	/// <returns>Percentage of target frame time used (0-100+).</returns>
	public double GetFrameTimeUsage(double targetFrameTime)
	{
		if (targetFrameTime <= 0)
		{
			return 0;
		}

		return (_averageFrameTime / targetFrameTime) * 100.0;
	}

	/// <summary>
	/// Records a new frame time sample.
	/// </summary>
	/// <param name="frameTime">Frame time in milliseconds.</param>
	public void RecordFrame(double frameTime)
	{
		_frameTimes.Enqueue(frameTime);
		_frameCount++;

		// Keep only the last N samples
		while (_frameTimes.Count > _maxSamples)
		{
			_frameTimes.Dequeue();
		}

		// Calculate average frame time
		if (_frameTimes.Count > 0)
		{
			_averageFrameTime = _frameTimes.Average();
		}

		// Calculate FPS (update every second)
		var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		var timeSinceLastUpdate = currentTime - _lastUpdateTime;

		if (timeSinceLastUpdate >= 1000) // Update every second
		{
			_fps = _frameCount / (timeSinceLastUpdate / 1000.0);
			_frameCount = 0;
			_lastUpdateTime = currentTime;
		}
	}

	/// <summary>
	/// Resets all metrics.
	/// </summary>
	public void Reset()
	{
		_frameTimes.Clear();
		_frameCount = 0;
		_fps = 0;
		_averageFrameTime = 0;
		_lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		DrawCalls = 0;
		TriangleCount = 0;
	}
}
