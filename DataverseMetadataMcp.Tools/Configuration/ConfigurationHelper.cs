using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DataverseMetadataMcp.Tools.Configuration;

/// <summary>
/// Helper class to access configuration from static methods in MCP tools
/// </summary>
public static class ConfigurationHelper
{
    private static DataverseConnectionService? _connectionService;

    /// <summary>
    /// Initialize the configuration helper with the connection service
    /// </summary>
    /// <param name="connectionService">The Dataverse connection service instance</param>
    public static void Initialize(DataverseConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    /// <summary>
    /// Gets the validated Dataverse connection string
    /// </summary>
    /// <returns>The connection string</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is not initialized</exception>
    public static string GetConnectionString()
    {
        if (_connectionService == null)
            throw new InvalidOperationException("Configuration not initialized");

        return _connectionService.ConnectionString;
    }

    /// <summary>
    /// Gets the shared ServiceClient instance
    /// </summary>
    /// <returns>The ServiceClient instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is not initialized</exception>
    public static ServiceClient GetServiceClient()
    {
        if (_connectionService == null)
            throw new InvalidOperationException("Configuration not initialized");

        return _connectionService.ServiceClient;
    }
}
