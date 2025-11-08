using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataverseMetadataMcp.Tools.Configuration;

namespace DataverseMetadataMcp.Server.Services;

/// <summary>
/// Background service that validates Dataverse connection on startup
/// </summary>
public class DataverseStartupService : BackgroundService
{
    private readonly DataverseConnectionService _connectionService;
    private readonly ILogger<DataverseStartupService> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public DataverseStartupService(
        DataverseConnectionService connectionService,
        ILogger<DataverseStartupService> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _connectionService = connectionService;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Dataverse connection validation...");

            var isValid = await _connectionService.ValidateConnectionAsync();

            if (!isValid)
            {
                _logger.LogError("Dataverse connection validation failed. Shutting down application.");
                _applicationLifetime.StopApplication();
                return;
            }

            _logger.LogInformation("Dataverse connection validation completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Dataverse connection validation");
            _applicationLifetime.StopApplication();
        }
    }
}
