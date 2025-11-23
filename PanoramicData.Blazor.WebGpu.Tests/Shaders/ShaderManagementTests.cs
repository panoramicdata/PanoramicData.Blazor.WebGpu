using Microsoft.JSInterop;
using Moq;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Resources;
using PanoramicData.Blazor.WebGpu.Services;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;
using PanoramicData.Blazor.WebGpu.Utilities;

namespace PanoramicData.Blazor.WebGpu.Tests.Shaders;

/// <summary>
/// Tests for shader management functionality.
/// </summary>
public class ShaderManagementTests : TestBase
{
	private Mock<IJSRuntime> CreateMockJSRuntime()
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

		mockModule
			.Setup(x => x.InvokeAsync<int>("createShaderModuleAsync", It.IsAny<object[]>()))
			.ReturnsAsync((string method, object[] args) => new Random().Next(1, 1000));

		return mockJsRuntime;
	}

	#region ShaderCompilationInfo Tests

	[Fact]
	public void ShaderCompilationInfo_Should_DefaultToSuccess()
	{
		// Act
		var info = new ShaderCompilationInfo();

		// Assert
		info.Success.Should().BeFalse(); // Default is false until set
		info.ErrorMessage.Should().BeNull();
		info.ErrorLine.Should().BeNull();
		info.ErrorColumn.Should().BeNull();
		info.Warnings.Should().BeEmpty();
	}

	[Fact]
	public void ShaderCompilationInfo_Should_StoreErrorInformation()
	{
		// Act
		var info = new ShaderCompilationInfo
		{
			Success = false,
			ErrorMessage = "Syntax error",
			ErrorLine = 42,
			ErrorColumn = 15
		};

		// Assert
		info.Success.Should().BeFalse();
		info.ErrorMessage.Should().Be("Syntax error");
		info.ErrorLine.Should().Be(42);
		info.ErrorColumn.Should().Be(15);
	}

	[Fact]
	public void ShaderCompilationInfo_Should_StoreWarnings()
	{
		// Act
		var info = new ShaderCompilationInfo
		{
			Success = true,
			Warnings = ["Warning 1", "Warning 2"]
		};

		// Assert
		info.Warnings.Should().HaveCount(2);
		info.Warnings.Should().Contain("Warning 1");
		info.Warnings.Should().Contain("Warning 2");
	}

	#endregion

	#region PDWebGpuShader.Validate Tests

	[Fact]
	public void Validate_Should_FailForEmptyShader()
	{
		// Act
		var result = PDWebGpuShader.Validate("");

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("empty");
		result.ErrorLine.Should().Be(1);
		result.ErrorColumn.Should().Be(1);
	}

	[Fact]
	public void Validate_Should_FailForWhitespaceOnlyShader()
	{
		// Act
		var result = PDWebGpuShader.Validate("   \n   \t   ");

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorMessage.Should().Contain("empty");
	}

	[Fact]
	public void Validate_Should_SucceedForValidShader()
	{
		// Arrange
		var shader = Infrastructure.Utilities.TestData.SimpleVertexShader;

		// Act
		var result = PDWebGpuShader.Validate(shader);

		// Assert
		result.Success.Should().BeTrue();
		result.ErrorMessage.Should().BeNull();
	}

	[Fact]
	public void Validate_Should_WarnAboutMisplacedAttributes()
	{
		// Arrange
		var shader = "fn main() @vertex -> vec4<f32> { return vec4<f32>(0.0); }";

		// Act
		var result = PDWebGpuShader.Validate(shader);

		// Assert
		result.Success.Should().BeTrue();
		result.Warnings.Should().NotBeEmpty();
		result.Warnings.Should().Contain(w => w.Contains("@vertex"));
	}

	#endregion

	#region ShaderLoader Tests

	[Fact]
	public async Task ShaderLoader_Should_LoadShader()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		// Act
		var shader = await loader.LoadShaderAsync("test", Infrastructure.Utilities.TestData.SimpleVertexShader);

		// Assert
		shader.Should().NotBeNull();
		shader.WgslCode.Should().Be(Infrastructure.Utilities.TestData.SimpleVertexShader);
		loader.IsLoaded("test").Should().BeTrue();

		await loader.DisposeAllAsync();
	}

	[Fact]
	public async Task ShaderLoader_Should_ThrowForEmptyName()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		// Act
		var act = async () => await loader.LoadShaderAsync("", "shader code");

		// Assert
		await act.Should().ThrowAsync<ArgumentException>().WithParameterName("name");
	}

	[Fact]
	public async Task ShaderLoader_Should_ReplaceExistingShader()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		var shader1 = "@vertex fn main() -> vec4<f32> { return vec4<f32>(1.0); }";
		var shader2 = "@vertex fn main() -> vec4<f32> { return vec4<f32>(2.0); }";

		// Act
		var firstShader = await loader.LoadShaderAsync("test", shader1);
		var secondShader = await loader.LoadShaderAsync("test", shader2);

		// Assert
		secondShader.WgslCode.Should().Be(shader2);
		loader.GetShaderSource("test").Should().Be(shader2);

		await loader.DisposeAllAsync();
	}

	[Fact]
	public async Task ShaderLoader_Should_ReloadShader()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		var reloadEventFired = false;
		string? reloadedShaderName = null;

		loader.ShaderReloaded += (s, e) =>
		{
			reloadEventFired = true;
			reloadedShaderName = e.ShaderName;
		};

		var originalShader = "@vertex fn main() -> vec4<f32> { return vec4<f32>(1.0); }";
		var newShader = "@vertex fn main() -> vec4<f32> { return vec4<f32>(2.0); }";

		await loader.LoadShaderAsync("test", originalShader);

		// Act
		var reloaded = await loader.ReloadShaderAsync("test", newShader);

		// Assert
		reloaded.WgslCode.Should().Be(newShader);
		reloadEventFired.Should().BeTrue();
		reloadedShaderName.Should().Be("test");

		await loader.DisposeAllAsync();
	}

	[Fact]
	public async Task ShaderLoader_Should_ThrowWhenReloadingNonExistentShader()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		// Act
		var act = async () => await loader.ReloadShaderAsync("nonexistent", "shader code");

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*has not been loaded*");
	}

	[Fact]
	public async Task ShaderLoader_Should_GetShaderByName()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		await loader.LoadShaderAsync("test", Infrastructure.Utilities.TestData.SimpleVertexShader);

		// Act
		var shader = loader.GetShader("test");

		// Assert
		shader.Should().NotBeNull();
		shader!.WgslCode.Should().Be(Infrastructure.Utilities.TestData.SimpleVertexShader);

		await loader.DisposeAllAsync();
	}

	[Fact]
	public void ShaderLoader_Should_ReturnNullForNonExistentShader()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var loader = new ShaderLoader(service);

		// Act
		var shader = loader.GetShader("nonexistent");

		// Assert
		shader.Should().BeNull();
	}

	[Fact]
	public async Task ShaderLoader_Should_GetShaderSource()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		var shaderCode = "@vertex fn main() -> vec4<f32> { return vec4<f32>(1.0); }";
		await loader.LoadShaderAsync("test", shaderCode);

		// Act
		var source = loader.GetShaderSource("test");

		// Assert
		source.Should().Be(shaderCode);

		await loader.DisposeAllAsync();
	}

	[Fact]
	public async Task ShaderLoader_Should_ListLoadedShaders()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		await loader.LoadShaderAsync("shader1", "@vertex fn main1() -> vec4<f32> { return vec4<f32>(1.0); }");
		await loader.LoadShaderAsync("shader2", "@fragment fn main2() -> vec4<f32> { return vec4<f32>(1.0); }");

		// Act
		var names = loader.GetLoadedShaderNames();

		// Assert
		names.Should().HaveCount(2);
		names.Should().Contain("shader1");
		names.Should().Contain("shader2");

		await loader.DisposeAllAsync();
	}

	[Fact]
	public async Task ShaderLoader_Should_DisposeAllShaders()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		await service.InitializeAsync();
		var loader = new ShaderLoader(service);

		await loader.LoadShaderAsync("shader1", "@vertex fn main1() -> vec4<f32> { return vec4<f32>(1.0); }");
		await loader.LoadShaderAsync("shader2", "@fragment fn main2() -> vec4<f32> { return vec4<f32>(1.0); }");

		// Act
		await loader.DisposeAllAsync();

		// Assert
		loader.GetLoadedShaderNames().Should().BeEmpty();
		loader.GetShader("shader1").Should().BeNull();
		loader.GetShader("shader2").Should().BeNull();
	}

	#endregion

	#region ShaderReloadedEventArgs Tests

	[Fact]
	public void ShaderReloadedEventArgs_Should_StoreData()
	{
		// Arrange
		var mockJs = CreateMockJSRuntime();
		var service = new PDWebGpuService(mockJs.Object);
		var shader = new PDWebGpuShader(service, 42, "shader code");

		// Act
		var args = new ShaderReloadedEventArgs("test", shader);

		// Assert
		args.ShaderName.Should().Be("test");
		args.Shader.Should().Be(shader);
	}

	#endregion
}
