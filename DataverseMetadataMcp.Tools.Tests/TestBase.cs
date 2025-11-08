using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using DataverseMetadataMcp.Tools;
using DataverseMetadataMcp.Tools.Configuration;
using Xunit;

namespace DataverseMetadataMcp.Tools.Tests;

/// <summary>
/// Base class for integration tests that provides shared Dataverse connection
/// </summary>
public abstract class TestBase : IAsyncDisposable
{
    private static readonly object _lockObject = new();
    private static ServiceProvider? _sharedServiceProvider;
    private static DataverseConnectionService? _sharedConnectionService;
    private static bool _connectionInitialized = false; protected ServiceProvider ServiceProvider { get; private set; } = null!;
    protected DataverseConnectionService ConnectionService { get; private set; } = null!;
    protected ServiceClient ServiceClient { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;

    protected TestBase()
    {
        InitializeServices();
    }

    private void InitializeServices()
    {
        lock (_lockObject)
        {
            if (!_connectionInitialized)
            {
                try
                {                    // Build configuration from various sources
                    var configBuilder = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddUserSecrets<TestBase>();

                    var tempConfig = configBuilder.Build();

                    // Validate that we have a connection string
                    var connectionString = tempConfig["DATAVERSE_CONNECTION_STRING"]
                        ?? tempConfig["ConnectionString"];

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new SkipException(
                            "No Dataverse connection string found. Please set DATAVERSE_CONNECTION_STRING environment variable " +
                            "or add it to user secrets with key 'ConnectionString'. " +
                            "Example: AuthType=OAuth;Url=https://yourorg.crm4.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/");
                    }

                    // Create in-memory configuration with the correct key for DataverseConnectionService
                    var inMemorySettings = new Dictionary<string, string?>
                    {
                        { "ConnectionString", connectionString }
                    };

                    var configuration = new ConfigurationBuilder()
                        .AddEnvironmentVariables()
                        .AddUserSecrets<TestBase>()
                        .AddInMemoryCollection(inMemorySettings)
                        .Build();

                    // Create service collection
                    var services = new ServiceCollection();

                    // Add configuration
                    services.AddSingleton<IConfiguration>(configuration);

                    // Add logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });

                    // Add application lifetime (required by DataverseConnectionService)
                    services.AddSingleton<IHostApplicationLifetime, TestHostApplicationLifetime>();

                    // Add Power Platform MCP tools
                    services.AddPowerPlatformMcpTools();

                    // Build service provider
                    _sharedServiceProvider = services.BuildServiceProvider();
                    _sharedConnectionService = _sharedServiceProvider.GetRequiredService<DataverseConnectionService>();

                    // Initialize and validate connection
                    var isValid = _sharedConnectionService.ValidateConnectionAsync().GetAwaiter().GetResult();
                    if (!isValid)
                    {
                        throw new SkipException("Failed to establish connection to Dataverse. Please check your connection string.");
                    }

                    // Initialize the configuration helper
                    PowerPlatformMcpToolsInitializer.Initialize(_sharedConnectionService);

                    _connectionInitialized = true;
                }
                catch (SkipException)
                {
                    throw; // Re-throw skip exceptions
                }
                catch (Exception ex)
                {
                    throw new SkipException($"Failed to initialize Dataverse connection: {ex.Message}");
                }
            }
        }

        if (_sharedServiceProvider == null || _sharedConnectionService == null)
        {
            throw new SkipException("Dataverse connection not available");
        }

        ServiceProvider = _sharedServiceProvider;
        ConnectionService = _sharedConnectionService;
        ServiceClient = ConnectionService.ServiceClient;
        Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();
    }

    /// <summary>
    /// Verifies that Dataverse connection is available and ready
    /// </summary>
    protected void EnsureDataverseConnection()
    {
        if (!ServiceClient.IsReady)
        {
            throw new SkipException("Dataverse connection is not ready");
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        // Individual test instances don't dispose the shared resources
        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the shared resources - call this when all tests are complete
    /// </summary>
    public static void DisposeSharedResources()
    {
        lock (_lockObject)
        {
            _sharedConnectionService?.Dispose();
            _sharedServiceProvider?.Dispose();
            _sharedConnectionService = null;
            _sharedServiceProvider = null;
            _connectionInitialized = false;
        }
    }
}

/// <summary>
/// Test implementation of IHostApplicationLifetime
/// </summary>
internal class TestHostApplicationLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted { get; } = new CancellationToken(false);
    public CancellationToken ApplicationStopping { get; } = new CancellationToken(false);
    public CancellationToken ApplicationStopped { get; } = new CancellationToken(false);

    public void StopApplication()
    {
        // No-op for tests
    }
}

/// <summary>
/// Exception thrown to skip a test when dependencies are not available
/// </summary>
public class SkipException : Exception
{
    public SkipException(string message) : base(message) { }
}
