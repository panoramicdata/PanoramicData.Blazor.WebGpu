using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using PanoramicData.Blazor.WebGpu.Extensions;
using PanoramicData.Blazor.WebGpu.Services;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Extensions;

/// <summary>
/// Tests for service collection extensions.
/// </summary>
public class ServiceCollectionExtensionsTests : TestBase
{
	[Fact]
	public void AddPDWebGpu_Should_RegisterService()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddScoped<IJSRuntime>(_ => new Mock<IJSRuntime>().Object);

		// Act
		services.AddPDWebGpu();

		// Assert
		var serviceProvider = services.BuildServiceProvider();
		var service = serviceProvider.GetService<IPDWebGpuService>();

		service.Should().NotBeNull();
		service.Should().BeOfType<PDWebGpuService>();
	}

	[Fact]
	public void AddPDWebGpu_Should_RegisterAsScoped()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddScoped<IJSRuntime>(_ => new Mock<IJSRuntime>().Object);
		services.AddPDWebGpu();

		// Act
		var serviceProvider = services.BuildServiceProvider();
		
		var scope1 = serviceProvider.CreateScope();
		var service1a = scope1.ServiceProvider.GetService<IPDWebGpuService>();
		var service1b = scope1.ServiceProvider.GetService<IPDWebGpuService>();

		var scope2 = serviceProvider.CreateScope();
		var service2 = scope2.ServiceProvider.GetService<IPDWebGpuService>();

		// Assert
		service1a.Should().BeSameAs(service1b); // Same within scope
		service1a.Should().NotBeSameAs(service2); // Different across scopes

		// Cleanup
		scope1.Dispose();
		scope2.Dispose();
		serviceProvider.Dispose();
	}

	[Fact]
	public void AddPDWebGpu_Should_ReturnServiceCollection_ForChaining()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		var result = services.AddPDWebGpu();

		// Assert
		result.Should().BeSameAs(services);
	}

	[Fact]
	public void AddPDWebGpu_Should_AllowMultipleCalls()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddScoped<IJSRuntime>(_ => new Mock<IJSRuntime>().Object);

		// Act
		services.AddPDWebGpu();
		services.AddPDWebGpu();
		services.AddPDWebGpu();

		// Assert
		var serviceProvider = services.BuildServiceProvider();
		var service = serviceProvider.GetService<IPDWebGpuService>();
		service.Should().NotBeNull();
	}
}
