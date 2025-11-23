using Microsoft.JSInterop;
using Moq;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Resources;
using PanoramicData.Blazor.WebGpu.Services;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Resources;

/// <summary>
/// Tests for WebGPU resource wrapper classes.
/// </summary>
public class ResourceTests : TestBase
{
	private static Mock<IJSRuntime> CreateMockJSRuntime()
	{
		var mockJsRuntime = new Mock<IJSRuntime>();
		var mockModule = new Mock<IJSObjectReference>();

		mockJsRuntime
			.Setup(x => x.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()))
			.ReturnsAsync(mockModule.Object);

		mockModule
			.Setup(x => x.InvokeAsync<bool>("isSupported", It.IsAny<object[]>()))
			.ReturnsAsync(true);

		mockModule
			.Setup(x => x.InvokeAsync<WebGpuDeviceInfo>("initializeAsync", It.IsAny<object[]>()))
			.ReturnsAsync(new WebGpuDeviceInfo
			{
				AdapterInfo = new AdapterInfo { Vendor = "Test" }
			});

		return mockJsRuntime;
	}

	#region PDWebGpuBuffer Tests

	[Fact]
	public void PDWebGpuBuffer_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var buffer = new PDWebGpuBuffer(service, 42, BufferType.Vertex, 1024);

		// Assert
		buffer.Should().NotBeNull();
		buffer.BufferType.Should().Be(BufferType.Vertex);
		buffer.Size.Should().Be(1024);
		buffer.ResourceId.Should().Be(42);
		buffer.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public void PDWebGpuBuffer_Should_SupportAllBufferTypes()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act & Assert
		var vertexBuffer = new PDWebGpuBuffer(service, 1, BufferType.Vertex, 100);
		vertexBuffer.BufferType.Should().Be(BufferType.Vertex);

		var indexBuffer = new PDWebGpuBuffer(service, 2, BufferType.Index, 200);
		indexBuffer.BufferType.Should().Be(BufferType.Index);

		var uniformBuffer = new PDWebGpuBuffer(service, 3, BufferType.Uniform, 300);
		uniformBuffer.BufferType.Should().Be(BufferType.Uniform);

		var storageBuffer = new PDWebGpuBuffer(service, 4, BufferType.Storage, 400);
		storageBuffer.BufferType.Should().Be(BufferType.Storage);
	}

	[Fact]
	public async Task PDWebGpuBuffer_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var buffer = new PDWebGpuBuffer(service, 42, BufferType.Vertex, 1024);

		// Act
		await buffer.DisposeAsync();

		// Assert
		buffer.IsDisposed.Should().BeTrue();
	}

	[Fact]
	public void PDWebGpuBuffer_Should_DisposeSynchronously()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var buffer = new PDWebGpuBuffer(service, 42, BufferType.Vertex, 1024);

		// Act
		buffer.Dispose();

		// Assert
		buffer.IsDisposed.Should().BeTrue();
	}

	[Fact]
	public async Task PDWebGpuBuffer_Should_BeIdempotentOnDispose()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var buffer = new PDWebGpuBuffer(service, 42, BufferType.Vertex, 1024);

		// Act
		await buffer.DisposeAsync();
		await buffer.DisposeAsync();

		// Assert
		buffer.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region PDWebGpuShader Tests

	[Fact]
	public void PDWebGpuShader_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var wgslCode = "@vertex\nfn main() -> @builtin(position) vec4<f32> { return vec4<f32>(0.0); }";

		// Act
		var shader = new PDWebGpuShader(service, 42, wgslCode);

		// Assert
		shader.Should().NotBeNull();
		shader.WgslCode.Should().Be(wgslCode);
		shader.ResourceId.Should().Be(42);
		shader.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public async Task PDWebGpuShader_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var shader = new PDWebGpuShader(service, 42, "shader code");

		// Act
		await shader.DisposeAsync();

		// Assert
		shader.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region PDWebGpuTexture Tests

	[Fact]
	public void PDWebGpuTexture_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var texture = new PDWebGpuTexture(service, 42, 1024, 768, TextureFormat.RGBA8Unorm);

		// Assert
		texture.Should().NotBeNull();
		texture.Width.Should().Be(1024);
		texture.Height.Should().Be(768);
		texture.Format.Should().Be(TextureFormat.RGBA8Unorm);
		texture.ResourceId.Should().Be(42);
		texture.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public void PDWebGpuTexture_Should_SupportDifferentFormats()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act & Assert
		var rgba = new PDWebGpuTexture(service, 1, 100, 100, TextureFormat.RGBA8Unorm);
		rgba.Format.Should().Be(TextureFormat.RGBA8Unorm);

		var bgra = new PDWebGpuTexture(service, 2, 100, 100, TextureFormat.BGRA8Unorm);
		bgra.Format.Should().Be(TextureFormat.BGRA8Unorm);

		var depth = new PDWebGpuTexture(service, 3, 100, 100, TextureFormat.Depth32Float);
		depth.Format.Should().Be(TextureFormat.Depth32Float);
	}

	[Fact]
	public async Task PDWebGpuTexture_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var texture = new PDWebGpuTexture(service, 42, 1024, 768, TextureFormat.RGBA8Unorm);

		// Act
		await texture.DisposeAsync();

		// Assert
		texture.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region PDWebGpuSampler Tests

	[Fact]
	public void PDWebGpuSampler_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var sampler = new PDWebGpuSampler(service, 42,
			FilterMode.Linear, FilterMode.Linear,
			AddressMode.Repeat, AddressMode.Repeat);

		// Assert
		sampler.Should().NotBeNull();
		sampler.MagFilter.Should().Be(FilterMode.Linear);
		sampler.MinFilter.Should().Be(FilterMode.Linear);
		sampler.AddressModeU.Should().Be(AddressMode.Repeat);
		sampler.AddressModeV.Should().Be(AddressMode.Repeat);
		sampler.ResourceId.Should().Be(42);
		sampler.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public void PDWebGpuSampler_Should_SupportDifferentFilterModes()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var nearestSampler = new PDWebGpuSampler(service, 1,
			FilterMode.Nearest, FilterMode.Nearest,
			AddressMode.ClampToEdge, AddressMode.ClampToEdge);

		var linearSampler = new PDWebGpuSampler(service, 2,
			FilterMode.Linear, FilterMode.Linear,
			AddressMode.ClampToEdge, AddressMode.ClampToEdge);

		// Assert
		nearestSampler.MagFilter.Should().Be(FilterMode.Nearest);
		linearSampler.MagFilter.Should().Be(FilterMode.Linear);
	}

	[Fact]
	public async Task PDWebGpuSampler_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var sampler = new PDWebGpuSampler(service, 42,
			FilterMode.Linear, FilterMode.Linear,
			AddressMode.Repeat, AddressMode.Repeat);

		// Act
		await sampler.DisposeAsync();

		// Assert
		sampler.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region PDWebGpuPipeline Tests

	[Fact]
	public void PDWebGpuPipeline_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var pipeline = new PDWebGpuPipeline(service, 42, PipelineType.Render);

		// Assert
		pipeline.Should().NotBeNull();
		pipeline.PipelineType.Should().Be(PipelineType.Render);
		pipeline.ResourceId.Should().Be(42);
		pipeline.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public void PDWebGpuPipeline_Should_SupportBothPipelineTypes()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var renderPipeline = new PDWebGpuPipeline(service, 1, PipelineType.Render);
		var computePipeline = new PDWebGpuPipeline(service, 2, PipelineType.Compute);

		// Assert
		renderPipeline.PipelineType.Should().Be(PipelineType.Render);
		computePipeline.PipelineType.Should().Be(PipelineType.Compute);
	}

	[Fact]
	public async Task PDWebGpuPipeline_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var pipeline = new PDWebGpuPipeline(service, 42, PipelineType.Render);

		// Act
		await pipeline.DisposeAsync();

		// Assert
		pipeline.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region PDWebGpuBindGroup Tests

	[Fact]
	public void PDWebGpuBindGroup_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var bindGroup = new PDWebGpuBindGroup(service, 42);

		// Assert
		bindGroup.Should().NotBeNull();
		bindGroup.ResourceId.Should().Be(42);
		bindGroup.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public async Task PDWebGpuBindGroup_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var bindGroup = new PDWebGpuBindGroup(service, 42);

		// Act
		await bindGroup.DisposeAsync();

		// Assert
		bindGroup.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region PDWebGpuCommandEncoder Tests

	[Fact]
	public void PDWebGpuCommandEncoder_Should_BeCreatable()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);

		// Act
		var encoder = new PDWebGpuCommandEncoder(service, 42);

		// Assert
		encoder.Should().NotBeNull();
		encoder.ResourceId.Should().Be(42);
		encoder.IsDisposed.Should().BeFalse();
	}

	[Fact]
	public void PDWebGpuCommandEncoder_Should_Finish()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var encoder = new PDWebGpuCommandEncoder(service, 42);

		// Act
		var commandBufferId = encoder.Finish();

		// Assert
		commandBufferId.Should().Be(42);
	}

	[Fact]
	public async Task PDWebGpuCommandEncoder_Should_ThrowWhenFinishingAfterDisposal()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var encoder = new PDWebGpuCommandEncoder(service, 42);
		await encoder.DisposeAsync();

		// Act
		var act = () => encoder.Finish();

		// Assert
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
	public async Task PDWebGpuCommandEncoder_Should_DisposeAsync()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var encoder = new PDWebGpuCommandEncoder(service, 42);

		// Act
		await encoder.DisposeAsync();

		// Assert
		encoder.IsDisposed.Should().BeTrue();
	}

	#endregion

	#region Enum Tests

	[Fact]
	public void BufferType_Should_HaveAllValues()
	{
		// This ensures all buffer types are defined
		Enum.GetValues<BufferType>().Should().Contain(BufferType.Vertex);
		Enum.GetValues<BufferType>().Should().Contain(BufferType.Index);
		Enum.GetValues<BufferType>().Should().Contain(BufferType.Uniform);
		Enum.GetValues<BufferType>().Should().Contain(BufferType.Storage);
	}

	[Fact]
	public void TextureFormat_Should_HaveAllValues()
	{
		// This ensures all texture formats are defined
		Enum.GetValues<TextureFormat>().Should().Contain(TextureFormat.RGBA8Unorm);
		Enum.GetValues<TextureFormat>().Should().Contain(TextureFormat.BGRA8Unorm);
		Enum.GetValues<TextureFormat>().Should().Contain(TextureFormat.Depth24PlusStencil8);
		Enum.GetValues<TextureFormat>().Should().Contain(TextureFormat.Depth32Float);
	}

	[Fact]
	public void FilterMode_Should_HaveAllValues()
	{
		// This ensures all filter modes are defined
		Enum.GetValues<FilterMode>().Should().Contain(FilterMode.Nearest);
		Enum.GetValues<FilterMode>().Should().Contain(FilterMode.Linear);
	}

	[Fact]
	public void AddressMode_Should_HaveAllValues()
	{
		// This ensures all address modes are defined
		Enum.GetValues<AddressMode>().Should().Contain(AddressMode.ClampToEdge);
		Enum.GetValues<AddressMode>().Should().Contain(AddressMode.Repeat);
		Enum.GetValues<AddressMode>().Should().Contain(AddressMode.MirrorRepeat);
	}

	[Fact]
	public void PipelineType_Should_HaveAllValues()
	{
		// This ensures all pipeline types are defined
		Enum.GetValues<PipelineType>().Should().Contain(PipelineType.Render);
		Enum.GetValues<PipelineType>().Should().Contain(PipelineType.Compute);
	}

	#endregion
}
