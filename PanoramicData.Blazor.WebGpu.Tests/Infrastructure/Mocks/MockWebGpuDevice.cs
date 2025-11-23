using Microsoft.JSInterop;
using Moq;

namespace PanoramicData.Blazor.WebGpu.Tests.Infrastructure.Mocks;

/// <summary>
/// Mock WebGPU device for testing without requiring actual GPU hardware.
/// </summary>
public class MockWebGpuDevice
{
	public Mock<IJSObjectReference> DeviceReference { get; }
	public Mock<IJSObjectReference> AdapterReference { get; }
	public Mock<IJSObjectReference> CanvasContextReference { get; }

	public MockWebGpuDevice()
	{
		DeviceReference = new Mock<IJSObjectReference>();
		AdapterReference = new Mock<IJSObjectReference>();
		CanvasContextReference = new Mock<IJSObjectReference>();

		SetupDefaultBehavior();
	}

	private void SetupDefaultBehavior()
	{
		// Setup typical WebGPU device responses
		DeviceReference
			.Setup(x => x.InvokeAsync<object>(
				"createBuffer",
				It.IsAny<object[]>()))
			.ReturnsAsync(new Mock<IJSObjectReference>().Object);

		DeviceReference
			.Setup(x => x.InvokeAsync<object>(
				"createShaderModule",
				It.IsAny<object[]>()))
			.ReturnsAsync(new Mock<IJSObjectReference>().Object);

		DeviceReference
			.Setup(x => x.InvokeAsync<object>(
				"createRenderPipeline",
				It.IsAny<object[]>()))
			.ReturnsAsync(new Mock<IJSObjectReference>().Object);
	}

	/// <summary>
	/// Simulates a shader compilation error.
	/// </summary>
	public void SimulateShaderCompilationError(string errorMessage)
	{
		DeviceReference
			.Setup(x => x.InvokeAsync<object>(
				"createShaderModule",
				It.IsAny<object[]>()))
			.ThrowsAsync(new JSException(errorMessage));
	}

	/// <summary>
	/// Simulates a device lost error.
	/// </summary>
	public void SimulateDeviceLost()
	{
		DeviceReference
			.Setup(x => x.InvokeAsync<object>(
				It.IsAny<string>(),
				It.IsAny<object[]>()))
			.ThrowsAsync(new JSException("Device lost"));
	}
}
