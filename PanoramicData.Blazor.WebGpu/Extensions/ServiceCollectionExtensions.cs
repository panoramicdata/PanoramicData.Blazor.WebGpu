using Microsoft.Extensions.DependencyInjection;
using PanoramicData.Blazor.WebGpu.Services;

namespace PanoramicData.Blazor.WebGpu.Extensions;

/// <summary>
/// Extension methods for registering WebGPU services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds WebGPU services to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for chaining.</returns>
	public static IServiceCollection AddPDWebGpu(this IServiceCollection services)
	{
		services.AddScoped<IPDWebGpuService, PDWebGpuService>();
		return services;
	}
}
