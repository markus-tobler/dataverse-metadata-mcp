using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using DataverseMetadataMcp.Tools;
using DataverseMetadataMcp.Tools.Configuration;
using DataverseMetadataMcp.Server.Services;

// Create a generic host builder for
// dependency injection, logging, and configuration.
var builder = Host.CreateApplicationBuilder(args);

// Add command line configuration
builder.Configuration.AddCommandLine(args, new Dictionary<string, string>
{
    { "--connection-string", "ConnectionString" },
    { "-cs", "ConnectionString" }
});

// Configure logging for better integration with MCP clients.
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register configuration as a service
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register Power Platform MCP tools and dependencies
builder.Services.AddPowerPlatformMcpTools();

// Register the Dataverse startup service specific to hosting
builder.Services.AddHostedService<DataverseStartupService>();

// Register the MCP server and configure it to use stdio transport.
// Scan both the current assembly and the Tools assembly for tool definitions.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithToolsFromAssembly(typeof(PowerPlatformMcpToolsInitializer).Assembly);

// Build the host
var host = builder.Build();

// Initialize the configuration helper
PowerPlatformMcpToolsInitializer.Initialize(host.Services.GetRequiredService<DataverseConnectionService>());

// Run the host. This starts the MCP server.
await host.RunAsync();