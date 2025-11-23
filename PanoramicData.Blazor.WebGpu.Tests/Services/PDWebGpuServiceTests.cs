using Microsoft.JSInterop;
using Moq;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Services;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Services;

/// <summary>
/// Tests for PDWebGpuService.
/// </summary>
public class PDWebGpuServiceTests : TestBase
{
	private static readonly string[] DepthClipControlArray = new[] { "depth-clip-control" };

	private static Mock<IJSRuntime> CreateMockJSRuntime(bool isSupported = true)
	{
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<bool>("isSupported", It.IsAny<object[]>()))
			.ReturnsAsync(isSupported);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuCompatibilityInfo>(
				"getCompatibilityInfo",
				It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuCompatibilityInfo
			{
				IsSupported = isSupported,
				UserAgent = "Test Browser",
				Vendor = "Test",
				Platform = "Test Platform"
			});

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>(
				"initializeAsync",
				It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuDeviceInfo
			{
				AdapterInfo = new AdapterInfo
				{
					Vendor = "Test Vendor",
					Device = "Test Device"
				},
				Features = DepthClipControlArray
			});

		return mockJsRuntime;
	}

	[Fact]
	public void Service_Should_BeCreatable()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();

		// Act
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Assert
		service.Should().NotBeNull();
		service.IsInitialized.Should().BeFalse();
		service.DeviceInfo.Should().BeNull();
	}

	[Fact]
	public async Task IsSupportedAsync_Should_ReturnTrue_When_WebGpuSupported()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime(true);
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		var isSupported = await service.IsSupportedAsync();

		// Assert
		isSupported.Should().BeTrue();
		service.IsSupported.Should().BeTrue();
	}

	[Fact]
	public async Task IsSupportedAsync_Should_ReturnFalse_When_WebGpuNotSupported()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime(false);
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		var isSupported = await service.IsSupportedAsync();

		// Assert
		isSupported.Should().BeFalse();
		service.IsSupported.Should().BeFalse();
	}

	[Fact]
	public async Task GetCompatibilityInfoAsync_Should_ReturnInfo()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		var info = await service.GetCompatibilityInfoAsync();

		// Assert
		info.Should().NotBeNull();
		info.UserAgent.Should().Be("Test Browser");
		service.CompatibilityInfo.Should().Be(info);
		service.IsSupported.Should().BeTrue();
	}

	[Fact]
	public async Task InitializeAsync_Should_InitializeDevice()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		var deviceReadyFired = false;

		service.DeviceReady += (s, e) => deviceReadyFired = true;

		// Act
		await service.InitializeAsync();

		// Assert
		service.IsInitialized.Should().BeTrue();
		service.DeviceInfo.Should().NotBeNull();
		service.DeviceInfo!.AdapterInfo.Vendor.Should().Be("Test Vendor");
		deviceReadyFired.Should().BeTrue();
	}

	[Fact]
	public async Task InitializeAsync_Should_ThrowPDWebGpuNotSupportedException_When_NotSupported()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime(false);
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		var act = async () => await service.InitializeAsync();

		// Assert
		await act.Should().ThrowExactlyAsync<PDWebGpuNotSupportedException>();
		service.IsInitialized.Should().BeFalse();
	}

	[Fact]
	public async Task InitializeAsync_Should_BeIdempotent()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		var deviceReadyCount = 0;

		service.DeviceReady += (s, e) => deviceReadyCount++;

		// Act
		await service.InitializeAsync();
		await service.InitializeAsync();
		await service.InitializeAsync();

		// Assert
		service.IsInitialized.Should().BeTrue();
		deviceReadyCount.Should().Be(1); // Should only fire once
	}

	[Fact]
	public async Task EnsureInitializedAsync_Should_Initialize_When_NotInitialized()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		await service.EnsureInitializedAsync();

		// Assert
		service.IsInitialized.Should().BeTrue();
	}

	[Fact]
	public async Task EnsureInitializedAsync_Should_NotReinitialize_When_AlreadyInitialized()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		var deviceReadyCount = 0;

		service.DeviceReady += (s, e) => deviceReadyCount++;

		await service.InitializeAsync();

		// Act
		await service.EnsureInitializedAsync();
		await service.EnsureInitializedAsync();

		// Assert
		deviceReadyCount.Should().Be(1);
	}

	[Fact]
	public async Task GetCanvasContextAsync_Should_ThrowArgumentException_When_CanvasIdEmpty()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		var act = async () => await service.GetCanvasContextAsync("");

		// Assert
		await act.Should().ThrowExactlyAsync<ArgumentException>()
			.WithParameterName("canvasId");
	}

	[Fact]
	public async Task GetCanvasContextAsync_Should_ReturnContextId()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		// Setup all required calls for initialization
		mockModule
			.Setup(x => x.InvokeAsync<bool>("isSupported", It.IsAny<object[]>()))
			.ReturnsAsync(true);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuCompatibilityInfo>(
				"getCompatibilityInfo",
				It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuCompatibilityInfo { IsSupported = true });

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>(
				"initializeAsync",
				It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuDeviceInfo
			{
				AdapterInfo = new AdapterInfo { Vendor = "Test" }
			});

		mockModule
			.Setup(x => x.InvokeAsync<CanvasContextResult>(
				"getCanvasContext",
				It.IsAny<object[]>()))
			.ReturnsAsync(new CanvasContextResult { ContextId = "canvas-1" });

		var service = new PDWebGpuService(mockJsRuntime.Object);
		await service.InitializeAsync();

		// Act
		var contextId = await service.GetCanvasContextAsync("canvas-1");

		// Assert
		contextId.Should().Be("canvas-1");
	}

	[Fact]
	public async Task CreateShaderModuleAsync_Should_ThrowArgumentException_When_WgslCodeEmpty()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);

		// Act
		var act = async () => await service.CreateShaderModuleAsync("");

		// Assert
		await act.Should().ThrowExactlyAsync<ArgumentException>()
			.WithParameterName("wgslCode");
	}

	[Fact]
	public async Task CreateShaderModuleAsync_Should_ReturnResourceId()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		// Setup all required calls for initialization
		mockModule
			.Setup(x => x.InvokeAsync<bool>("isSupported", It.IsAny<object[]>()))
			.ReturnsAsync(true);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuCompatibilityInfo>(
				"getCompatibilityInfo",
				It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuCompatibilityInfo { IsSupported = true });

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>(
				"initializeAsync",
				It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuDeviceInfo
			{
				AdapterInfo = new AdapterInfo { Vendor = "Test" }
			});

		mockModule
			.Setup(x => x.InvokeAsync<int>(
				"createShaderModuleAsync",
				It.IsAny<object[]>()))
			.ReturnsAsync(42);

		var service = new PDWebGpuService(mockJsRuntime.Object);
		await service.InitializeAsync();

		// Act
		var resourceId = await service.CreateShaderModuleAsync(Infrastructure.Utilities.TestData.SimpleVertexShader);

		// Assert
		resourceId.Should().Be(42);
	}

	[Fact]
	public async Task SubmitCommandBuffersAsync_Should_NotThrow_When_EmptyArray()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		await service.InitializeAsync();

		// Act
		var act = async () => await service.SubmitCommandBuffersAsync();

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task SubmitCommandBuffersAsync_Should_CallInterop()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		await service.InitializeAsync();

		// Act
		var act = async () => await service.SubmitCommandBuffersAsync(1, 2, 3);

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task ErrorEvent_Should_FireOnError()
	{
		// Arrange
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>(
				"import",
				It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		// Setup GetCompatibilityInfoAsync to succeed
		mockModule
			.Setup(x => x.InvokeAsync<WebGpuCompatibilityInfo>("getCompatibilityInfo", It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuCompatibilityInfo { IsSupported = true });

		// Setup InitializeAsync to succeed
		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>("initializeAsync", It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuDeviceInfo { AdapterInfo = new AdapterInfo { Vendor = "Test" } });

		// Setup GetCanvasContextAsync to fail
		mockModule
			.Setup(x => x.InvokeAsync<CanvasContextResult>("getCanvasContext", It.IsAny<object[]>()))
			.ThrowsAsync(new JSException("Test error"));

		var service = new PDWebGpuService(mockJsRuntime.Object);
		PDWebGpuErrorEventArgs? errorArgs = null;

		service.Error += (s, e) => errorArgs = e;

		await service.InitializeAsync();

		// Act
		try
		{
			await service.GetCanvasContextAsync("test-canvas");
		}
		catch
		{
			// Expected exception
		}

		// Assert
		errorArgs.Should().NotBeNull();
		errorArgs!.Exception.Should().BeOfType<PDWebGpuException>();
	}

	[Fact]
	public async Task DisposeAsync_Should_Cleanup()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		await service.InitializeAsync();

		// Act
		await service.DisposeAsync();

		// Assert
		service.IsInitialized.Should().BeFalse();
		service.DeviceInfo.Should().BeNull();
	}

	[Fact]
	public async Task DisposeAsync_Should_BeIdempotent()
	{
		// Arrange
		var mockJsRuntime = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJsRuntime.Object);
		await service.InitializeAsync();

		// Act
		await service.DisposeAsync();
		var act = async () => await service.DisposeAsync();

		// Assert
		await act.Should().NotThrowAsync();
	}
}
