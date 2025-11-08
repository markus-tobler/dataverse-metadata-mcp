using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DataverseMetadataMcp.Tools.Configuration;

/// <summary>
/// Service for managing Dataverse connections and validation
/// </summary>
public class DataverseConnectionService : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataverseConnectionService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private string? _connectionString;
    private ServiceClient? _serviceClient;
    private readonly object _lockObject = new object();

    public DataverseConnectionService(
        IConfiguration configuration,
        ILogger<DataverseConnectionService> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _configuration = configuration;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    /// <summary>
    /// Gets the validated connection string
    /// </summary>
    public string ConnectionString => _connectionString
        ?? throw new InvalidOperationException("Connection string not validated yet");

    /// <summary>
    /// Gets the established ServiceClient instance
    /// </summary>
    public ServiceClient ServiceClient => _serviceClient
        ?? throw new InvalidOperationException("ServiceClient not established yet. Call ValidateConnectionAsync first.");

    /// <summary>
    /// Validates the Dataverse connection at startup
    /// </summary>
    /// <returns>True if connection is valid, false otherwise</returns>
    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            // Try to get connection string from configuration
            _connectionString = _configuration["ConnectionString"];

            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogError("No connection string provided. Please provide --connection-string or -cs argument when starting the server.");
                return false;
            }

            // Create the ServiceClient (this may trigger interactive authentication)
            _logger.LogInformation("Establishing Dataverse connection...");
            _logger.LogInformation("Note: If using OAuth, an authentication dialog may appear.");

            lock (_lockObject)
            {
                _serviceClient = new ServiceClient(_connectionString);
            }

            if (!_serviceClient.IsReady)
            {
                _logger.LogError("Failed to connect to Dataverse: {Error}", _serviceClient.LastError);
                return false;
            }

            // Perform simple validation to ensure connection works
            var connectionValid = await ValidateServiceClientAsync(_serviceClient);
            if (connectionValid)
            {
                _logger.LogInformation("Successfully connected to Dataverse!");
                return true;
            }

            _logger.LogError("Connection validation failed");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Dataverse connection");
            return false;
        }
    }

    /// <summary>
    /// Validates the ServiceClient connection by testing basic operations
    /// </summary>
    private async Task<bool> ValidateServiceClientAsync(ServiceClient serviceClient)
    {
        try
        {
            // Test the connection by retrieving the organization entity
            // This is a simple operation that validates connectivity
            var orgQuery = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet("name", "organizationid"),
                TopCount = 1
            };

            var result = await serviceClient.RetrieveMultipleAsync(orgQuery);

            if (result.Entities.Count > 0)
            {
                var orgEntity = result.Entities[0];
                var orgName = orgEntity.GetAttributeValue<string>("name");
                var orgId = orgEntity.GetAttributeValue<Guid>("organizationid");

                _logger.LogInformation("Connected to organization: {OrgName} (ID: {OrgId})", orgName, orgId);
                return true;
            }

            _logger.LogWarning("Organization query returned no results");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection validation failed");
            return false;
        }
    }

    /// <summary>
    /// Attempts to create a connection string through interactive login
    /// </summary>
    private Task<string?> GetInteractiveConnectionStringAsync()
    {
        try
        {
            // Prompt user for environment URL
            Console.Write("Enter your Dataverse environment URL (e.g., https://yourorg.crm.dynamics.com): ");
            var environmentUrl = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(environmentUrl))
            {
                _logger.LogWarning("No environment URL provided");
                return Task.FromResult<string?>(null);
            }

            // Ensure URL format is correct
            if (!environmentUrl.StartsWith("http"))
            {
                environmentUrl = "https://" + environmentUrl;
            }

            // Create connection string for interactive login
            var connectionString = $"Url={environmentUrl};AuthType=OAuth;RedirectUri=http://localhost;LoginPrompt=Auto;";

            _logger.LogInformation("Attempting interactive authentication...");

            return Task.FromResult<string?>(connectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during interactive connection setup");
            return Task.FromResult<string?>(null);
        }
    }

    /// <summary>
    /// Disposes the ServiceClient and releases resources
    /// </summary>
    public void Dispose()
    {
        lock (_lockObject)
        {
            _serviceClient?.Dispose();
            _serviceClient = null;
        }
    }
}
