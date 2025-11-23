using Microsoft.JSInterop;
using Moq;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Interop;

/// <summary>
/// Tests for WebGPU JavaScript interop functionality.
/// </summary>
public class WebGpuJsInteropTests : TestBase
{
	[Fact]
	public async Task IsSupportedAsync_Should_ReturnTrue_When_WebGpuAvailable()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<bool>("isSupported", It.IsAny<object[]>()))
			.ReturnsAsync(true);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act
		var isSupported = await interop.IsSupportedAsync();

		// Assert
		isSupported.Should().BeTrue();
	}

	[Fact]
	public async Task IsSupportedAsync_Should_ThrowPDWebGpuException_When_JsError()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<bool>("isSupported", It.IsAny<object[]>()))
			.ThrowsAsync(new JSException("JavaScript error"));

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act & Assert
		var act = async () => await interop.IsSupportedAsync();
		await act.Should().ThrowExactlyAsync<PDWebGpuException>()
			.WithMessage("*WebGPU support*");
	}

	[Fact]
	public async Task GetCompatibilityInfoAsync_Should_ReturnInfo()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		var expectedInfo = new WebGpuCompatibilityInfo
		{
			IsSupported = true,
			UserAgent = "Test Browser",
			Vendor = "Test",
			Platform = "Test Platform"
		};

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuCompatibilityInfo>(
				"getCompatibilityInfo",
				It.IsAny<object[]>()))
			.ReturnsAsync(expectedInfo);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act
		var info = await interop.GetCompatibilityInfoAsync();

		// Assert
		info.Should().NotBeNull();
		info.IsSupported.Should().BeTrue();
		info.UserAgent.Should().Be("Test Browser");
	}

	[Fact]
	public async Task InitializeAsync_Should_ReturnDeviceInfo_When_Successful()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		var expectedDeviceInfo = new WebGpuDeviceInfo
		{
			AdapterInfo = new AdapterInfo
			{
				Vendor = "Test Vendor",
				Device = "Test Device"
			},
			Features = new[] { "depth-clip-control", "texture-compression-bc" }
		};

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>(
				"initializeAsync",
				It.IsAny<object[]>()))
			.ReturnsAsync(expectedDeviceInfo);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act
		var deviceInfo = await interop.InitializeAsync();

		// Assert
		deviceInfo.Should().NotBeNull();
		deviceInfo.AdapterInfo.Vendor.Should().Be("Test Vendor");
		deviceInfo.Features.Should().Contain("depth-clip-control");
	}

	[Fact]
	public async Task InitializeAsync_Should_ThrowPDWebGpuDeviceException_When_DeviceInitFails()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>(
				"initializeAsync",
				It.IsAny<object[]>()))
			.ThrowsAsync(new JSException("Failed to get WebGPU adapter"));

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act & Assert
		var act = async () => await interop.InitializeAsync();
		await act.Should().ThrowExactlyAsync<PDWebGpuDeviceException>()
			.WithMessage("*initialize WebGPU device*");
	}

	[Fact]
	public async Task GetCanvasContextAsync_Should_ReturnContextId()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<CanvasContextResult>(
				"getCanvasContext",
				It.IsAny<object[]>()))
			.ReturnsAsync(new CanvasContextResult { ContextId = "canvas-1" });

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act
		var contextId = await interop.GetCanvasContextAsync("canvas-1");

		// Assert
		contextId.Should().Be("canvas-1");
	}

	[Fact]
	public async Task CreateShaderModuleAsync_Should_ReturnResourceId()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<int>(
				"createShaderModuleAsync",
				It.IsAny<object[]>()))
			.ReturnsAsync(42);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act
		var resourceId = await interop.CreateShaderModuleAsync(Infrastructure.Utilities.TestData.SimpleVertexShader);

		// Assert
		resourceId.Should().Be(42);
	}

	[Fact]
	public async Task CreateShaderModuleAsync_Should_ThrowPDWebGpuShaderCompilationException_When_CompilationFails()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<int>(
				"createShaderModuleAsync",
				It.IsAny<object[]>()))
			.ThrowsAsync(new JSException("Line 3: syntax error"));

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Act & Assert
		var act = async () => await interop.CreateShaderModuleAsync(Infrastructure.Utilities.TestData.InvalidShader);
		await act.Should().ThrowExactlyAsync<PDWebGpuShaderCompilationException>()
			.WithMessage("*compilation failed*");
	}

	[Fact]
	public async Task SubmitCommandBuffersAsync_Should_NotThrow()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);
		var commandBufferIds = new[] { 1, 2, 3 };

		// Act
		var act = async () => await interop.SubmitCommandBuffersAsync(commandBufferIds);

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task DisposeAsync_Should_NotThrow()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);

		// Force module initialization
		await interop.IsSupportedAsync();

		// Act
		var act = async () => await interop.DisposeAsync();

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task DisposeAsync_Should_BeIdempotent()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		var interop = new WebGpuJsInterop(mockJsRuntime.Object);
		await interop.IsSupportedAsync();

		// Act - calling twice should not throw
		await interop.DisposeAsync();
		var act = async () => await interop.DisposeAsync();

		// Assert
		await act.Should().NotThrowAsync();
	}
}
