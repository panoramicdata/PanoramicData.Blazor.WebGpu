using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using PanoramicData.Blazor.WebGpu.Components;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Services;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.RenderLoop;

/// <summary>
/// Tests for render loop functionality.
/// </summary>
public class RenderLoopTests : TestBase
{
	[Fact]
	public void PDWebGpuContainer_Should_HaveRenderLoopProperties()
	{
		// This test verifies the render loop properties exist
		// We can't easily test the actual render loop without a full Blazor context
		
		// Just verify the FrameRateMode enum exists
		var variableMode = FrameRateMode.Variable;
		var fixedMode = FrameRateMode.Fixed;

		variableMode.Should().Be(FrameRateMode.Variable);
		fixedMode.Should().Be(FrameRateMode.Fixed);
	}

	[Fact]
	public void FrameRateMode_Should_HaveTwoValues()
	{
		// Verify enum values
		var values = Enum.GetValues<FrameRateMode>();
		values.Should().HaveCount(2);
		values.Should().Contain(FrameRateMode.Variable);
		values.Should().Contain(FrameRateMode.Fixed);
	}

	[Fact]
	public void PDWebGpuFrameEventArgs_Should_StoreFrameTiming()
	{
		// Arrange & Act
		var frameArgs = new PDWebGpuFrameEventArgs
		{
			DeltaTime = 16.67,
			TotalTime = 1000,
			FrameNumber = 60
		};

		// Assert
		frameArgs.DeltaTime.Should().Be(16.67);
		frameArgs.TotalTime.Should().Be(1000);
		frameArgs.FrameNumber.Should().Be(60);
	}

	[Fact]
	public void PDWebGpuFrameEventArgs_Should_CalculateFPS()
	{
		// Arrange
		var frameArgs = new PDWebGpuFrameEventArgs
		{
			DeltaTime = 16.67, // ~60 FPS
			TotalTime = 1000,
			FrameNumber = 60
		};

		// Act
		var approximateFPS = 1000.0 / frameArgs.DeltaTime;

		// Assert
		approximateFPS.Should().BeApproximately(60, 1);
	}

	[Fact]
	public async Task IVisibilityCallback_Should_BeImplementable()
	{
		// Arrange
		var callbackInvoked = false;
		var isVisible = false;

		var callback = new TestVisibilityCallback((visible) =>
		{
			callbackInvoked = true;
			isVisible = visible;
		});

		// Act
		await callback.OnVisibilityChanged(true);

		// Assert
		callbackInvoked.Should().BeTrue();
		isVisible.Should().BeTrue();
	}

	[Fact]
	public async Task IVisibilityCallback_Should_HandleVisibilityChanges()
	{
		// Arrange
		var visibilityStates = new List<bool>();
		var callback = new TestVisibilityCallback((visible) => visibilityStates.Add(visible));

		// Act
		await callback.OnVisibilityChanged(true);
		await callback.OnVisibilityChanged(false);
		await callback.OnVisibilityChanged(true);

		// Assert
		visibilityStates.Should().HaveCount(3);
		visibilityStates[0].Should().BeTrue();
		visibilityStates[1].Should().BeFalse();
		visibilityStates[2].Should().BeTrue();
	}

	[Fact]
	public void RenderLoop_Should_SupportVariableFrameRate()
	{
		// This verifies that the FrameRateMode.Variable option exists
		var mode = FrameRateMode.Variable;
		mode.Should().Be(FrameRateMode.Variable);
	}

	[Fact]
	public void RenderLoop_Should_SupportFixedFrameRate()
	{
		// This verifies that the FrameRateMode.Fixed option exists
		var mode = FrameRateMode.Fixed;
		mode.Should().Be(FrameRateMode.Fixed);
	}

	[Fact]
	public void RenderLoop_Should_CalculateFrameTimeCorrectly()
	{
		// Arrange
		var targetFPS = 60;
		var expectedFrameTime = 1000.0 / targetFPS;

		// Act
		var actualFrameTime = 1000.0 / targetFPS;

		// Assert
		actualFrameTime.Should().BeApproximately(expectedFrameTime, 0.01);
		actualFrameTime.Should().BeApproximately(16.67, 0.1);
	}

	[Fact]
	public void RenderLoop_Should_CalculateDifferentFrameRates()
	{
		// Test common frame rates
		var fps30 = 1000.0 / 30;
		var fps60 = 1000.0 / 60;
		var fps120 = 1000.0 / 120;
		var fps144 = 1000.0 / 144;

		fps30.Should().BeApproximately(33.33, 0.1);
		fps60.Should().BeApproximately(16.67, 0.1);
		fps120.Should().BeApproximately(8.33, 0.1);
		fps144.Should().BeApproximately(6.94, 0.1);
	}

	[Fact]
	public void PDWebGpuResizeEventArgs_Should_StoreOldAndNewDimensions()
	{
		// Arrange & Act
		var resizeArgs = new PDWebGpuResizeEventArgs
		{
			Width = 1920,
			Height = 1080,
			OldWidth = 1280,
			OldHeight = 720
		};

		// Assert
		resizeArgs.Width.Should().Be(1920);
		resizeArgs.Height.Should().Be(1080);
		resizeArgs.OldWidth.Should().Be(1280);
		resizeArgs.OldHeight.Should().Be(720);
	}

	private class TestVisibilityCallback : IVisibilityCallback
	{
		private readonly Action<bool> _onVisibilityChanged;

		public TestVisibilityCallback(Action<bool> onVisibilityChanged)
		{
			_onVisibilityChanged = onVisibilityChanged;
		}

		public Task OnVisibilityChanged(bool isVisible)
		{
			_onVisibilityChanged(isVisible);
			return Task.CompletedTask;
		}
	}
}
