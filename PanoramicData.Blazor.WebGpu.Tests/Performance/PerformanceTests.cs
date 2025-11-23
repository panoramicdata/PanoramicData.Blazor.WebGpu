using PanoramicData.Blazor.WebGpu.Performance;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Performance;

/// <summary>
/// Tests for performance metrics functionality.
/// </summary>
public class PerformanceTests : TestBase
{
	#region PDWebGpuPerformanceDisplayOptions Tests

	[Fact]
	public void PerformanceDisplayOptions_Should_HaveDefaultValues()
	{
		// Act
		var options = new PDWebGpuPerformanceDisplayOptions();

		// Assert
		options.ShowFPS.Should().BeTrue();
		options.ShowFrameTime.Should().BeTrue();
		options.ShowFrameTimeUsage.Should().BeFalse();
		options.ShowDrawCalls.Should().BeFalse();
		options.ShowTriangleCount.Should().BeFalse();
		options.Position.Should().Be(CornerPosition.TopRight);
		options.BackgroundOpacity.Should().Be(0.7);
		options.UpdateIntervalMs.Should().Be(500);
		options.CustomMetrics.Should().BeEmpty();
	}

	[Fact]
	public void PerformanceDisplayOptions_Should_AllowCustomization()
	{
		// Act
		var options = new PDWebGpuPerformanceDisplayOptions
		{
			ShowFPS = false,
			ShowFrameTime = false,
			ShowFrameTimeUsage = true,
			ShowDrawCalls = true,
			ShowTriangleCount = true,
			Position = CornerPosition.BottomLeft,
			BackgroundOpacity = 0.5,
			UpdateIntervalMs = 1000
		};

		// Assert
		options.ShowFPS.Should().BeFalse();
		options.ShowFrameTimeUsage.Should().BeTrue();
		options.Position.Should().Be(CornerPosition.BottomLeft);
		options.BackgroundOpacity.Should().Be(0.5);
	}

	[Fact]
	public void PerformanceDisplayOptions_Should_SupportCustomMetrics()
	{
		// Act
		var options = new PDWebGpuPerformanceDisplayOptions();
		options.CustomMetrics["Memory"] = () => "128 MB";
		options.CustomMetrics["GPU Load"] = () => "75%";

		// Assert
		options.CustomMetrics.Should().HaveCount(2);
		options.CustomMetrics["Memory"]().Should().Be("128 MB");
		options.CustomMetrics["GPU Load"]().Should().Be("75%");
	}

	#endregion

	#region CornerPosition Tests

	[Fact]
	public void CornerPosition_Should_HaveAllValues()
	{
		// Assert
		Enum.GetValues<CornerPosition>().Should().Contain(CornerPosition.TopLeft);
		Enum.GetValues<CornerPosition>().Should().Contain(CornerPosition.TopRight);
		Enum.GetValues<CornerPosition>().Should().Contain(CornerPosition.BottomLeft);
		Enum.GetValues<CornerPosition>().Should().Contain(CornerPosition.BottomRight);
	}

	#endregion

	#region PerformanceMetrics Tests

	[Fact]
	public void PerformanceMetrics_Should_InitializeToZero()
	{
		// Act
		var metrics = new PerformanceMetrics();

		// Assert
		metrics.FPS.Should().Be(0);
		metrics.AverageFrameTime.Should().Be(0);
		metrics.DrawCalls.Should().Be(0);
		metrics.TriangleCount.Should().Be(0);
	}

	[Fact]
	public void PerformanceMetrics_Should_RecordFrameTime()
	{
		// Arrange
		var metrics = new PerformanceMetrics();

		// Act
		metrics.RecordFrame(16.67); // 60 FPS frame time
		metrics.RecordFrame(16.67);
		metrics.RecordFrame(16.67);

		// Assert
		metrics.AverageFrameTime.Should().BeApproximately(16.67, 0.01);
	}

	[Fact]
	public void PerformanceMetrics_Should_CalculateAverageFrameTime()
	{
		// Arrange
		var metrics = new PerformanceMetrics();

		// Act
		metrics.RecordFrame(10);
		metrics.RecordFrame(20);
		metrics.RecordFrame(30);

		// Assert
		metrics.AverageFrameTime.Should().Be(20);
	}

	[Fact]
	public void PerformanceMetrics_Should_LimitSampleSize()
	{
		// Arrange
		var metrics = new PerformanceMetrics();

		// Act - Record more than max samples (60)
		for (var i = 0; i < 100; i++)
		{
			metrics.RecordFrame(16.67);
		}

		// Assert - Should not crash and should have reasonable average
		metrics.AverageFrameTime.Should().BeApproximately(16.67, 0.01);
	}

	[Fact]
	public void PerformanceMetrics_Should_CalculateFPS()
	{
		// Arrange
		var metrics = new PerformanceMetrics();

		// Act - Simulate 1 second of 60 FPS rendering
		for (var i = 0; i < 60; i++)
		{
			metrics.RecordFrame(16.67);
			Thread.Sleep(17); // Approximate 60 FPS timing
		}

		// Assert - FPS should be calculated (may not be exactly 60 due to timing)
		metrics.FPS.Should().BeGreaterThan(0);
	}

	[Fact]
	public void PerformanceMetrics_Should_CalculateFrameTimeUsage()
	{
		// Arrange
		var metrics = new PerformanceMetrics();
		metrics.RecordFrame(16.67); // 60 FPS frame time

		// Act
		var usage = metrics.GetFrameTimeUsage(16.67); // 60 FPS target

		// Assert - Should be approximately 100%
		usage.Should().BeApproximately(100, 1);
	}

	[Fact]
	public void PerformanceMetrics_Should_CalculateFrameTimeUsageForSlowFrames()
	{
		// Arrange
		var metrics = new PerformanceMetrics();
		metrics.RecordFrame(33.34); // 30 FPS frame time

		// Act
		var usage = metrics.GetFrameTimeUsage(16.67); // 60 FPS target

		// Assert - Should be approximately 200% (twice the target)
		usage.Should().BeApproximately(200, 1);
	}

	[Fact]
	public void PerformanceMetrics_Should_HandleZeroTargetFrameTime()
	{
		// Arrange
		var metrics = new PerformanceMetrics();
		metrics.RecordFrame(16.67);

		// Act
		var usage = metrics.GetFrameTimeUsage(0);

		// Assert
		usage.Should().Be(0);
	}

	[Fact]
	public void PerformanceMetrics_Should_TrackDrawCalls()
	{
		// Arrange
		var metrics = new PerformanceMetrics();

		// Act
		metrics.DrawCalls = 42;

		// Assert
		metrics.DrawCalls.Should().Be(42);
	}

	[Fact]
	public void PerformanceMetrics_Should_TrackTriangleCount()
	{
		// Arrange
		var metrics = new PerformanceMetrics();

		// Act
		metrics.TriangleCount = 1_000_000;

		// Assert
		metrics.TriangleCount.Should().Be(1_000_000);
	}

	[Fact]
	public void PerformanceMetrics_Should_Reset()
	{
		// Arrange
		var metrics = new PerformanceMetrics();
		metrics.RecordFrame(16.67);
		metrics.RecordFrame(16.67);
		metrics.DrawCalls = 10;
		metrics.TriangleCount = 5000;

		// Act
		metrics.Reset();

		// Assert
		metrics.FPS.Should().Be(0);
		metrics.AverageFrameTime.Should().Be(0);
		metrics.DrawCalls.Should().Be(0);
		metrics.TriangleCount.Should().Be(0);
	}

	#endregion

	#region Frame Time Calculations

	[Fact]
	public void FrameTime_Should_CalculateFor30FPS()
	{
		// 30 FPS = 33.33ms per frame
		var frameTime = 1000.0 / 30;
		frameTime.Should().BeApproximately(33.33, 0.1);
	}

	[Fact]
	public void FrameTime_Should_CalculateFor60FPS()
	{
		// 60 FPS = 16.67ms per frame
		var frameTime = 1000.0 / 60;
		frameTime.Should().BeApproximately(16.67, 0.1);
	}

	[Fact]
	public void FrameTime_Should_CalculateFor144FPS()
	{
		// 144 FPS = 6.94ms per frame
		var frameTime = 1000.0 / 144;
		frameTime.Should().BeApproximately(6.94, 0.1);
	}

	#endregion

	#region Usage Calculations

	[Fact]
	public void Usage_Should_Be100PercentAtTarget()
	{
		// If frame time equals target, usage should be 100%
		var frameTime = 16.67;
		var targetTime = 16.67;
		var usage = (frameTime / targetTime) * 100;

		usage.Should().BeApproximately(100, 0.1);
	}

	[Fact]
	public void Usage_Should_Be50PercentWhenFast()
	{
		// If frame time is half of target, usage should be 50%
		var frameTime = 8.33;
		var targetTime = 16.67;
		var usage = (frameTime / targetTime) * 100;

		usage.Should().BeApproximately(50, 0.1);
	}

	[Fact]
	public void Usage_Should_Be200PercentWhenSlow()
	{
		// If frame time is double the target, usage should be 200%
		var frameTime = 33.34;
		var targetTime = 16.67;
		var usage = (frameTime / targetTime) * 100;

		usage.Should().BeApproximately(200, 0.1);
	}

	#endregion
}
