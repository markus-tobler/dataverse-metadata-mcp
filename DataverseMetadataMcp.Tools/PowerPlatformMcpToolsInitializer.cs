using DataverseMetadataMcp.Tools.Configuration;

namespace DataverseMetadataMcp.Tools;

/// <summary>
/// Initialization helper for Power Platform MCP Tools
/// </summary>
public static class PowerPlatformMcpToolsInitializer
{
    /// <summary>
    /// Initializes the configuration helper with the connection service
    /// </summary>
    /// <param name="connectionService">The Dataverse connection service instance</param>
    public static void Initialize(DataverseConnectionService connectionService)
    {
        ConfigurationHelper.Initialize(connectionService);
    }
}
