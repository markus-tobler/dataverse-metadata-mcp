using Microsoft.Extensions.DependencyInjection;
using DataverseMetadataMcp.Tools.Configuration;

namespace DataverseMetadataMcp.Tools;

/// <summary>
/// Extension methods for adding Power Platform MCP tools to the service collection
/// </summary>
public static class ServiceCollectionExtensions
{    /// <summary>
     /// Adds Power Platform MCP tools and their dependencies to the service collection
     /// </summary>
     /// <param name="services">The service collection</param>
     /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPowerPlatformMcpTools(this IServiceCollection services)
    {
        // Register Dataverse connection services
        services.AddSingleton<DataverseConnectionService>();

        return services;
    }
}
