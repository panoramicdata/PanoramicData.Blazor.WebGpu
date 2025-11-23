using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests;

/// <summary>
/// Tests for FrameRateMode enum.
/// </summary>
public class FrameRateModeTests : TestBase
{
	[Fact]
	public void FrameRateMode_Should_HaveVariableValue()
	{
		// Act
		var mode = FrameRateMode.Variable;

		// Assert
		mode.Should().Be(FrameRateMode.Variable);
		((int)mode).Should().Be(0);
	}

	[Fact]
	public void FrameRateMode_Should_HaveFixedValue()
	{
		// Act
		var mode = FrameRateMode.Fixed;

		// Assert
		mode.Should().Be(FrameRateMode.Fixed);
		((int)mode).Should().Be(1);
	}

	[Fact]
	public void FrameRateMode_Should_BeAssignable()
	{
		// Arrange
		var mode = FrameRateMode.Variable;

		// Act
		mode = FrameRateMode.Fixed;

		// Assert
		mode.Should().Be(FrameRateMode.Fixed);
	}
}
