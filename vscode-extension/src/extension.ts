import * as vscode from "vscode";
import * as path from "path";
import * as fs from "fs";
import { execSync } from "child_process";

/**
 * MCP Server Definition Provider for Dataverse Metadata
 *
 * This provider registers the Dataverse MCP server with VS Code,
 * allowing GitHub Copilot to access Dataverse metadata through MCP tools.
 */
class DataverseMcpServerProvider
  implements
    vscode.McpServerDefinitionProvider<vscode.McpStdioServerDefinition>
{
  private _onDidChangeMcpServerDefinitions = new vscode.EventEmitter<void>();
  readonly onDidChangeMcpServerDefinitions =
    this._onDidChangeMcpServerDefinitions.event;

  /**
   * Provides the list of available MCP servers.
   * This is called eagerly by VS Code to discover available servers.
   */
  async provideMcpServerDefinitions(
    token: vscode.CancellationToken
  ): Promise<vscode.McpStdioServerDefinition[]> {
    const config = vscode.workspace.getConfiguration("dataverseMetadataMcp");
    const enabled = config.get<boolean>("enabled", true);

    if (!enabled) {
      return [];
    }

    // Get connection string from configuration
    const connectionString = config.get<string>("connectionString", "");

    // Determine the server command path
    const serverPath = await this.getServerPath();

    if (!serverPath) {
      // If no server is available, return empty array
      // User will be prompted to configure when they try to use it
      return [];
    }

    // Create the MCP server definition
    const args: string[] = [];
    if (connectionString) {
      args.push("--connection-string", connectionString);
    }

    const server = new vscode.McpStdioServerDefinition(
      "Dataverse Metadata",
      serverPath,
      args,
      undefined, // env
      "2.3.0" // version from your csproj
    );

    return [server];
  }

  /**
   * Resolves the MCP server definition before starting.
   * This is where we can prompt for authentication or configuration.
   */
  async resolveMcpServerDefinition(
    server: vscode.McpStdioServerDefinition,
    token: vscode.CancellationToken
  ): Promise<vscode.McpStdioServerDefinition | undefined> {
    const config = vscode.workspace.getConfiguration("dataverseMetadataMcp");
    let connectionString = config.get<string>("connectionString", "");

    // If no connection string is configured, prompt the user
    if (!connectionString) {
      const result = await vscode.window.showWarningMessage(
        "Dataverse MCP Server requires a connection string. Would you like to configure it now?",
        "Configure",
        "Cancel"
      );

      if (result === "Configure") {
        const promptedConnectionString = await this.promptForConnectionString();
        if (!promptedConnectionString) {
          return undefined; // User cancelled
        }
        connectionString = promptedConnectionString;
      } else {
        return undefined;
      }
    }

    // Update the args with the connection string
    const args = ["--connection-string", connectionString];

    // Create a new server definition with the resolved connection string
    return new vscode.McpStdioServerDefinition(
      server.label,
      server.command,
      args,
      server.env,
      server.version
    );
  }

  /**
   * Determines the path to the MCP server executable
   */
  private async getServerPath(): Promise<string | null> {
    const config = vscode.workspace.getConfiguration("dataverseMetadataMcp");
    const customPath = config.get<string>("serverPath", "");

    // 1. Check for custom path in settings
    if (customPath && fs.existsSync(customPath)) {
      return customPath;
    }

    // 2. Check for bundled server in extension directory
    const bundledPath = this.getBundledServerPath();
    if (bundledPath && fs.existsSync(bundledPath)) {
      return bundledPath;
    }

    // 3. Check if dotnet tool is installed globally
    if (await this.isDotnetToolInstalled()) {
      return "dataverse-metadata-mcp-server";
    }

    // 4. No server found
    return null;
  }

  /**
   * Gets the path to the bundled MCP server executable
   */
  private getBundledServerPath(): string | null {
    const extensionPath = vscode.extensions.getExtension(
      "markus-tobler.dataverse-metadata-mcp"
    )?.extensionPath;
    if (!extensionPath) {
      return null;
    }

    // Determine platform-specific paths
    const platform = process.platform;
    let serverSubDir: string;
    let serverExeName: string;

    if (platform === "win32") {
      serverSubDir = "win-x64";
      serverExeName = "DataverseMetadataMcp.Server.exe";
    } else if (platform === "linux") {
      serverSubDir = "linux-x64";
      serverExeName = "DataverseMetadataMcp.Server";
    } else if (platform === "darwin") {
      serverSubDir = "osx-x64";
      serverExeName = "DataverseMetadataMcp.Server";
    } else {
      return null;
    }

    // Check for bundled server in platform-specific directory
    const bundledPath = path.join(
      extensionPath,
      "server",
      serverSubDir,
      serverExeName
    );
    if (fs.existsSync(bundledPath)) {
      return bundledPath;
    }

    // Fallback: check root server directory
    const possiblePaths = [
      path.join(extensionPath, "server", serverExeName),
      path.join(extensionPath, "dist", "server", serverExeName),
    ];

    for (const serverPath of possiblePaths) {
      if (fs.existsSync(serverPath)) {
        return serverPath;
      }
    }

    return null;
  }

  /**
   * Checks if the dataverse-metadata-mcp-server dotnet tool is installed globally
   */
  private async isDotnetToolInstalled(): Promise<boolean> {
    try {
      const output = execSync("dotnet tool list --global", {
        encoding: "utf-8",
      });
      return (
        output.includes("dataversemetadatamcp.server") ||
        output.includes("dataverse-metadata-mcp-server")
      );
    } catch (error) {
      return false;
    }
  }

  /**
   * Prompts the user to enter a connection string
   */
  private async promptForConnectionString(): Promise<string | undefined> {
    const input = await vscode.window.showInputBox({
      prompt: "Enter your Dataverse connection string",
      placeHolder:
        "AuthType=OAuth;Url=https://yourorg.crm.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/",
      ignoreFocusOut: true,
      validateInput: (value) => {
        if (!value || value.trim().length === 0) {
          return "Connection string cannot be empty";
        }
        if (!value.includes("Url=") || !value.includes("AuthType=")) {
          return "Connection string must include Url and AuthType";
        }
        return null;
      },
    });

    if (input) {
      // Save to configuration
      await vscode.workspace
        .getConfiguration("dataverseMetadataMcp")
        .update("connectionString", input, vscode.ConfigurationTarget.Global);
    }

    return input;
  }

  /**
   * Triggers a refresh of the server definitions
   */
  public refresh(): void {
    this._onDidChangeMcpServerDefinitions.fire();
  }
}

/**
 * Extension activation
 */
export function activate(context: vscode.ExtensionContext) {
  console.log("Dataverse Metadata MCP extension is now active");

  // Create the MCP server provider
  const provider = new DataverseMcpServerProvider();

  // Register the MCP server definition provider
  const disposable = vscode.lm.registerMcpServerDefinitionProvider(
    "dataverse-metadata-mcp.servers",
    provider
  );

  context.subscriptions.push(disposable);

  // Register command to configure connection
  const configureCommand = vscode.commands.registerCommand(
    "dataverseMetadataMcp.configure",
    async () => {
      const config = vscode.workspace.getConfiguration("dataverseMetadataMcp");
      const currentConnectionString = config.get<string>(
        "connectionString",
        ""
      );

      const input = await vscode.window.showInputBox({
        prompt: "Enter your Dataverse connection string",
        value: currentConnectionString,
        placeHolder:
          "AuthType=OAuth;Url=https://yourorg.crm.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/",
        ignoreFocusOut: true,
        validateInput: (value) => {
          if (!value || value.trim().length === 0) {
            return "Connection string cannot be empty";
          }
          if (!value.includes("Url=") || !value.includes("AuthType=")) {
            return "Connection string must include Url and AuthType";
          }
          return null;
        },
      });

      if (input) {
        await config.update(
          "connectionString",
          input,
          vscode.ConfigurationTarget.Global
        );
        vscode.window.showInformationMessage(
          "Dataverse connection string updated successfully"
        );
        provider.refresh();
      }
    }
  );

  context.subscriptions.push(configureCommand);

  // Register command to refresh servers
  const refreshCommand = vscode.commands.registerCommand(
    "dataverseMetadataMcp.refreshServers",
    () => {
      provider.refresh();
      vscode.window.showInformationMessage("MCP servers refreshed");
    }
  );

  context.subscriptions.push(refreshCommand);

  // Check if server is available and show setup message if not
  checkServerAvailability();
}

/**
 * Check if the MCP server is available and guide the user if not
 */
async function checkServerAvailability() {
  const config = vscode.workspace.getConfiguration("dataverseMetadataMcp");
  const customPath = config.get<string>("serverPath", "");

  let serverAvailable = false;

  // Check custom path
  if (customPath && fs.existsSync(customPath)) {
    serverAvailable = true;
  }

  // Check bundled server
  const extensionPath = vscode.extensions.getExtension(
    "markus-tobler.dataverse-metadata-mcp"
  )?.extensionPath;
  if (extensionPath && !serverAvailable) {
    const platform = process.platform;
    let serverSubDir: string;
    let serverExeName: string;

    if (platform === "win32") {
      serverSubDir = "win-x64";
      serverExeName = "DataverseMetadataMcp.Server.exe";
    } else if (platform === "linux") {
      serverSubDir = "linux-x64";
      serverExeName = "DataverseMetadataMcp.Server";
    } else if (platform === "darwin") {
      serverSubDir = "osx-x64";
      serverExeName = "DataverseMetadataMcp.Server";
    } else {
      serverSubDir = "";
      serverExeName = "DataverseMetadataMcp.Server";
    }

    const bundledPaths = [
      path.join(extensionPath, "server", serverSubDir, serverExeName),
      path.join(extensionPath, "server", serverExeName),
    ];

    for (const serverPath of bundledPaths) {
      if (fs.existsSync(serverPath)) {
        serverAvailable = true;
        break;
      }
    }
  }

  // Check dotnet tool
  if (!serverAvailable) {
    try {
      const output = execSync("dotnet tool list --global", {
        encoding: "utf-8",
      });
      serverAvailable =
        output.includes("dataversemetadatamcp.server") ||
        output.includes("dataverse-metadata-mcp-server");
    } catch (error) {
      // dotnet not available
    }
  }

  // Show message if server is not available
  if (!serverAvailable) {
    const choice = await vscode.window.showWarningMessage(
      "Dataverse MCP Server is not installed. Would you like to see installation instructions?",
      "Show Instructions",
      "Dismiss"
    );

    if (choice === "Show Instructions") {
      const installGuide = `
# Dataverse MCP Server Installation

To use the Dataverse Metadata MCP extension, you need to install the MCP server.

## Option 1: Install as .NET Global Tool (Recommended)

\`\`\`bash
dotnet tool install --global DataverseMetadataMcp.Server
\`\`\`

## Option 2: Set Custom Server Path

If you have built the server from source, you can set the path in VS Code settings:
1. Open Settings (Ctrl+,)
2. Search for "Dataverse Metadata MCP"
3. Set "Server Path" to your executable location

## Next Steps

After installation:
1. Configure your Dataverse connection string using the command palette:
   - Press Ctrl+Shift+P
   - Run "Dataverse MCP: Configure Dataverse Connection"
2. Restart VS Code
3. Use GitHub Copilot with Dataverse metadata tools!
      `.trim();

      const doc = await vscode.workspace.openTextDocument({
        content: installGuide,
        language: "markdown",
      });
      await vscode.window.showTextDocument(doc);
    }
  }
}

/**
 * Extension deactivation
 */
export function deactivate() {
  console.log("Dataverse Metadata MCP extension is now deactivated");
}
