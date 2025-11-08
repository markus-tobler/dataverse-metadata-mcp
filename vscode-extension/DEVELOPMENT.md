# VS Code Extension Development Guide

This guide covers developing, testing, and publishing the Dataverse Metadata MCP VS Code extension.

## Project Structure

```
vscode-extension/
├── src/
│   └── extension.ts          # Main extension code
├── scripts/
│   ├── build-with-server.ps1 # Build and bundle server
│   ├── publish.ps1           # Publish to marketplace
│   ├── install-local.ps1     # Install for local testing
│   └── create-icon.ps1       # Generate icon
├── .vscode/
│   ├── launch.json           # Debug configuration
│   ├── tasks.json            # Build tasks
│   └── settings.json         # Editor settings
├── dist/                     # Compiled output
├── server/                   # Bundled MCP server (optional)
├── package.json              # Extension manifest
├── tsconfig.json             # TypeScript config
├── esbuild.js               # Build configuration
└── README.md                # Extension documentation
```

## Prerequisites

- Node.js 20.x or later
- npm 10.x or later
- VS Code 1.96.0 or later
- .NET 9.0 SDK (for building the MCP server)

## Quick Start

### 1. Install Dependencies

```bash
cd vscode-extension
npm install
```

### 2. Compile the Extension

```bash
npm run compile
```

### 3. Test Locally

Press `F5` in VS Code to open the Extension Development Host with the extension loaded.

## Development Workflow

### Building

```bash
# Compile TypeScript and bundle
npm run compile

# Watch mode for development
npm run watch

# Production build
npm run package
```

### Testing

#### Manual Testing

1. Open the extension folder in VS Code
2. Press `F5` to launch Extension Development Host
3. In the new window:
   - Open Command Palette (Ctrl+Shift+P)
   - Run "Dataverse MCP: Configure Dataverse Connection"
   - Enter a connection string
   - Start using Copilot with Dataverse tools

#### Automated Testing

```bash
npm test
```

### Debugging

The extension includes debug configurations in `.vscode/launch.json`:

- **Run Extension**: Launch the extension in debug mode
- **Extension Tests**: Run and debug tests

Set breakpoints in `src/extension.ts` and use VS Code's debugger.

## Bundling the MCP Server

The extension can work in three modes:

1. **Global .NET Tool** (default): Uses `dataverse-metadata-mcp-server` installed globally
2. **Bundled Server**: Includes the compiled server in the extension
3. **Custom Path**: User-specified path to the server

### To Bundle the Server

```powershell
.\scripts\build-with-server.ps1
```

This script:

1. Builds the .NET server for Windows, Linux, and macOS
2. Copies binaries to `server/` directory
3. Compiles the extension
4. Packages everything as a `.vsix` file

The bundled `.vsix` will be larger but won't require users to install the .NET tool separately.

## Key Extension Features

### MCP Server Registration

The extension implements `McpServerDefinitionProvider`:

```typescript
class DataverseMcpServerProvider
  implements
    vscode.McpServerDefinitionProvider<vscode.McpStdioServerDefinition>
{
  provideMcpServerDefinitions(): Promise<vscode.McpStdioServerDefinition[]> {
    // Returns available MCP servers
  }

  resolveMcpServerDefinition(server): Promise<vscode.McpStdioServerDefinition> {
    // Resolves configuration before starting server
  }
}
```

### Configuration

The extension contributes settings:

- `dataverseMetadataMcp.enabled`: Enable/disable the server
- `dataverseMetadataMcp.connectionString`: Dataverse connection
- `dataverseMetadataMcp.serverPath`: Custom server path

### Commands

- `dataverseMetadataMcp.configure`: Configure connection string
- `dataverseMetadataMcp.refreshServers`: Refresh MCP servers

## Packaging

### Create VSIX Package

```bash
npx vsce package
```

This creates `dataverse-metadata-mcp-0.1.0.vsix`.

### Install Locally

```bash
code --install-extension dataverse-metadata-mcp-0.1.0.vsix
```

## Publishing to Marketplace

See [AUTOMATED-RELEASES.md](./AUTOMATED-RELEASES.md) for the automated release process.

### Quick Overview

1. Update version in `package.json`
2. Update `CHANGELOG.md`
3. Create PR to `main`
4. Merge PR → Automatic release and publish! ✨

## Troubleshooting

### Extension Not Activating

Check the Output panel:

1. View → Output
2. Select "Extension Host" from dropdown
3. Look for activation errors

### Server Not Found

The extension tries to locate the server in this order:

1. Custom path from settings
   The extension looks for the server in the bundled server directory.

Verify server availability:

```bash
# Check bundled server
ls ./server/
```

### TypeScript Errors

```bash
# Clean build
rm -rf dist node_modules
npm install
npm run compile
```

### vsce Package Errors

Common issues:

- Missing LICENSE file → Added automatically
- Missing icon.png → Run `.\scripts\create-icon.ps1`
- Large package size → Check `.vscodeignore`

## Architecture Notes

### Extension Activation

The extension activates `onStartupFinished` and:

1. Registers the MCP server definition provider
2. Checks if server is available
3. Shows setup instructions if needed

### Server Discovery

The extension uses platform-specific logic to find the bundled server:

```typescript
// Platform-specific paths
win - x64 / pp - mcp - server.exe;
linux - x64 / pp - mcp - server;
osx - x64 / pp - mcp - server;
```

### Connection String Management

Connection strings are stored in VS Code settings (Global scope) and passed to the server via command-line arguments.

## Best Practices

### Code Quality

- Run `npm run lint` before committing
- Use TypeScript strict mode
- Handle all error cases
- Provide user-friendly messages

### Security

- Never hardcode connection strings
- Use VS Code's SecretStorage for sensitive data if needed
- Validate user input

### Performance

- Lazy-load heavy dependencies
- Use esbuild for fast bundling
- Minimize extension activation time

### UX

- Provide clear error messages
- Guide users through setup
- Support multiple installation methods
- Test on all platforms

## Release Checklist

- [ ] Update version in `package.json`
- [ ] Update `CHANGELOG.md`
- [ ] Test on Windows, Linux, macOS
- [ ] Verify server detection works
- [ ] Test with and without bundled server
- [ ] Run `npm run lint`
- [ ] Run `npm test`
- [ ] Build and test VSIX locally
- [ ] Review README and documentation
- [ ] Tag release in git
- [ ] Publish to marketplace
- [ ] Verify marketplace listing

## Resources

- [VS Code Extension API](https://code.visualstudio.com/api)
- [MCP Server Definition Provider API](https://code.visualstudio.com/api/references/vscode-api#McpServerDefinitionProvider)
- [Extension Publishing](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Extension Guidelines](https://code.visualstudio.com/api/references/extension-guidelines)
