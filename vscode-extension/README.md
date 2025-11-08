# Dataverse Metadata MCP Extension

This VS Code extension provides access to Power Platform Dataverse metadata through the Model Context Protocol (MCP), enabling GitHub Copilot to assist with Dataverse development.

## Features

- **Automatic MCP Server Registration**: Seamlessly integrates the Dataverse MCP server with VS Code
- **GitHub Copilot Integration**: Use natural language to query and work with Dataverse metadata
- **Easy Configuration**: Simple setup through VS Code settings
- **Multiple Installation Options**: Bundle with extension or use globally installed .NET tool

## Requirements

- Visual Studio Code 1.96.0 or later
- .NET 9.0 SDK (if using the dotnet tool installation)
- GitHub Copilot extension
- Access to a Power Platform Dataverse environment

## Installation

### From VS Code Marketplace

1. Open VS Code
2. Go to Extensions (Ctrl+Shift+X)
3. Search for "Dataverse Metadata MCP"
4. Click Install

### From VSIX File

```bash
code --install-extension dataverse-metadata-mcp-0.1.0.vsix
```

## Setup

### Step 1: Install the MCP Server

The extension requires the `dataverse-metadata-mcp-server` to be installed. You have two options:

#### Option A: Install as .NET Global Tool (Recommended)

```bash
dotnet tool install --global DataverseMetadataMcp.Server
```

#### Option B: Use Custom Server Path

If you've built the server from source or have it in a custom location:

1. Open VS Code Settings (Ctrl+,)
2. Search for "Dataverse Metadata MCP"
3. Set "Server Path" to your executable location

### Step 2: Configure Dataverse Connection

1. Open the Command Palette (Ctrl+Shift+P)
2. Run "Dataverse MCP: Configure Dataverse Connection"
3. Enter your Dataverse connection string:

```
AuthType=OAuth;Url=https://yourorg.crm.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/
```

### Step 3: Verify Installation

1. Restart VS Code
2. Open a file in your workspace
3. Start a Copilot chat
4. Ask about Dataverse metadata (e.g., "List all tables in my Dataverse environment")

## Extension Settings

This extension contributes the following settings:

- `dataverseMetadataMcp.enabled`: Enable/disable the Dataverse MCP server
- `dataverseMetadataMcp.connectionString`: Dataverse connection string for authentication
- `dataverseMetadataMcp.serverPath`: Custom path to the dataverse-metadata-mcp-server executable (optional)

## Commands

- `Dataverse MCP: Configure Dataverse Connection`: Set up your Dataverse connection string
- `Dataverse MCP: Refresh MCP Servers`: Refresh the MCP server list

## Usage Examples

With this extension installed and configured, you can ask GitHub Copilot questions like:

- "Show me all the tables in my Dataverse environment"
- "What columns does the Account table have?"
- "List all business process flows"
- "Show me the relationships for the Contact table"
- "What are the global option sets in my environment?"

## Development

### Building from Source

1. Clone the repository:

```bash
git clone https://github.com/markus-tobler/dataverse-metadata-mcp.git
cd dataverse-metadata-mcp/vscode-extension
```

2. Install dependencies:

```bash
npm install
```

3. Compile the extension:

```bash
npm run compile
```

4. Press F5 to open a new VS Code window with the extension loaded

### Packaging

```bash
npm run package
vsce package
```

### Publishing

See [AUTOMATED-RELEASES.md](AUTOMATED-RELEASES.md) for the automated release process.

## Troubleshooting

### Server Not Found

If the extension cannot find the MCP server:

1. Verify the server is installed:

   ```bash
   dotnet tool list --global
   ```

2. Or set a custom path in settings:
   - Open Settings (Ctrl+,)
   - Search for "Dataverse Metadata MCP: Server Path"
   - Set the full path to your `dataverse-metadata-mcp-server` executable

### Connection Issues

If you're having trouble connecting to Dataverse:

1. Verify your connection string format
2. Ensure you have the necessary permissions in Dataverse
3. Check the VS Code Output panel for error messages

### MCP Tools Not Available in Copilot

1. Ensure the extension is enabled in settings
2. Restart VS Code
3. Check that your connection string is configured
4. Verify the server is running (check Output panel)

## Contributing

Contributions are welcome! Please see the [main repository](https://github.com/markus-tobler/dataverse-metadata-mcp) for contribution guidelines.

## License

MIT License - see [LICENSE](../LICENSE) for details

## Links

- [GitHub Repository](https://github.com/markus-tobler/dataverse-metadata-mcp)
- [Report Issues](https://github.com/markus-tobler/dataverse-metadata-mcp/issues)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Power Platform Documentation](https://learn.microsoft.com/power-platform/)

## Release Notes

### 0.1.0

Initial release of Dataverse Metadata MCP Extension

- MCP server registration with VS Code
- Connection string configuration
- Support for bundled or globally installed server
- Integration with GitHub Copilot
