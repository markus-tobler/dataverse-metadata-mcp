# Scripts for Extension Development

This directory contains helper scripts for building and managing the VS Code extension.

## Available Scripts

### build-with-server.ps1

Builds the extension and bundles the MCP server executable.

```powershell
.\scripts\build-with-server.ps1
```

This script:

1. Builds the .NET MCP server in Release mode
2. Publishes the server to the extension's `server` folder
3. Compiles the extension
4. Packages the extension as a .vsix file

### publish.ps1

Publishes the extension to the VS Code Marketplace.

```powershell
.\scripts\publish.ps1 -Version "0.1.0"
```

Options:

- `-Version`: The version to publish (e.g., "0.1.0", "patch", "minor", "major")
- `-SkipTests`: Skip running tests before publishing

### install-local.ps1

Installs the packaged extension locally for testing.

```powershell
.\scripts\install-local.ps1
```

## Usage

### Development Workflow

1. Make changes to the extension code
2. Test locally:

   ```powershell
   npm run compile
   # Press F5 in VS Code to launch Extension Development Host
   ```

3. Build with bundled server:

   ```powershell
   .\scripts\build-with-server.ps1
   ```

4. Install locally:

   ```powershell
   .\scripts\install-local.ps1
   ```

5. Publish to marketplace:
   ```powershell
   .\scripts\publish.ps1 -Version "0.2.0"
   ```
