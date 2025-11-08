# Dataverse Metadata MCP

An MCP (Model Context Protocol) implementation for Power Platform projects, providing tools to access Dataverse metadata.

## Installation

Install the **Dataverse Metadata MCP** extension from the VS Code Marketplace:

1. Open VS Code
2. Go to Extensions (Ctrl+Shift+X)
3. Search for "Dataverse Metadata MCP"
4. Click Install

The extension automatically registers the MCP server with GitHub Copilot and provides an easy-to-use configuration UI for setting up your Dataverse connection. See [vscode-extension/README.md](./vscode-extension/README.md) for details.

## Development Setup

If you want to contribute or run from source:

1. **Clone the repository**:

   ```powershell
   git clone https://github.com/markus-tobler/dataverse-metadata-mcp.git
   cd dataverse-metadata-mcp
   ```

2. **Build the solution**:

   ```powershell
   dotnet build
   ```

3. **Run the server with a connection string**:

   ```powershell
   dotnet run --project DataverseMetadataMcp.Server -- --connection-string "AuthType=OAuth;Url=https://yourorg.crm4.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/"
   ```

4. **For development with VS Code**, the extension can be configured to run the server from source. See [vscode-extension/DEVELOPMENT.md](./vscode-extension/DEVELOPMENT.md) for details.

## Project Structure

This solution contains multiple components:

### DataverseMetadataMcp.Server

A console application that hosts the MCP server with stdio transport. Bundled with the VS Code extension for easy deployment.

### DataverseMetadataMcp.Tools

A reusable class library containing the actual MCP tools for Power Platform operations. This library can be integrated into other MCP servers as well.

### vscode-extension

A VS Code extension that automatically registers the MCP server with GitHub Copilot. This provides the easiest setup experience for VS Code users. See [vscode-extension/README.md](./vscode-extension/README.md) for details.

**Key Features:**

- Automatic MCP server registration
- Easy configuration UI
- Bundled server binaries for all platforms
- Cross-platform support (Windows, Linux, macOS)

## Features

- **Comprehensive Dataverse Metadata Access**: Complete metadata retrieval including tables, relationships, choices, security, forms, workflows, and organizational information
- **Comprehensive Column Creation Tools**: Create new columns (attributes) in Dataverse tables with comprehensive validation, type checking, and naming convention enforcement - supports all major data types including text, numbers, choices, lookups, files, and more
- **Global Choice Creation**: Create new global choices (option sets) with custom options and validation
- **OAuth Authentication**: Supports modern authentication flows
- **Modular Design**: Tools are separated from the server for reusability
- **MCP Protocol**: Full compatibility with Model Context Protocol clients
- **VS Code Integration**: Seamless integration with GitHub Copilot in VS Code
- **Enterprise-Ready**: Security roles, solution management, and organizational insights

## Available Tools

### 📋 **Table and Column Metadata**

- `ReadTables()`: Retrieves all tables (entities) from Dataverse
- `ReadTable(tableName)`: Gets detailed information about a specific table
- `ReadColumns(tableName)`: Retrieves all columns (attributes) for a specific table

### ➕ **Column Creation Tools**

- `CreateTextColumn(tableName, columnName, displayName, description, ...)`: Creates a new text column with validation and formatting options
- `CreateIntegerColumn(tableName, columnName, displayName, description, ...)`: Creates a new whole number column with range validation
- `CreateDecimalColumn(tableName, columnName, displayName, description, ...)`: Creates a new decimal number column with precision control
- `CreateCurrencyColumn(tableName, columnName, displayName, description, ...)`: Creates a new currency column with proper formatting
- `CreateDateTimeColumn(tableName, columnName, displayName, description, ...)`: Creates a new date/time column with behavior options
- `CreateBooleanColumn(tableName, columnName, displayName, description, ...)`: Creates a new yes/no column with custom labels
- `CreateMultilineTextColumn(tableName, columnName, displayName, description, ...)`: Creates a new multiline text column
- `CreateLookupColumn(tableName, columnName, displayName, description, targetTableName, ...)`: Creates a new lookup column referencing another table
- `CreateChoiceColumn(tableName, columnName, displayName, description, choiceOptions, ...)`: Creates a new choice column with local options
- `CreateMultiSelectChoiceColumn(tableName, columnName, displayName, description, choiceOptions, ...)`: Creates a new multi-select choice column with local options
- `CreateGlobalChoiceColumn(tableName, columnName, displayName, description, globalChoiceName, ...)`: Creates a new choice column using an existing global choice
- `CreateGlobalMultiSelectChoiceColumn(tableName, columnName, displayName, description, globalChoiceName, ...)`: Creates a new multi-select choice column using an existing global choice
- `CreateFileColumn(tableName, columnName, displayName, description, ...)`: Creates a new file (image) column for storing image files
- `CreateFloatingPointColumn(tableName, columnName, displayName, description, ...)`: Creates a new floating point number column with precision control
- `CreateBigIntegerColumn(tableName, columnName, displayName, description, ...)`: Creates a new big integer column for large numbers
- `CreateCustomerColumn(tableName, columnName, displayName, description, ...)`: Creates a new customer column that can reference Account or Contact tables

### 🔗 **Relationship Metadata**

- `ReadEntityRelationships(tableName)`: Gets all relationships (1:N, N:1, N:N) for a table
- `ReadOneToManyRelationships(tableName)`: Gets one-to-many relationships
- `ReadManyToOneRelationships(tableName)`: Gets many-to-one relationships
- `ReadManyToManyRelationships(tableName)`: Gets many-to-many relationships

### 🎯 **Choice/Picklist Metadata**

- `CreateGlobalChoice(choiceName, displayName, description, choiceOptions)`: Creates a new global choice (option set) with custom options
- `ReadGlobalOptionSets()`: Gets all global choice sets
- `ReadGlobalOptionSet(optionSetName)`: Gets details of a specific global option set
- `ReadLocalOptionSets(tableName)`: Gets local option sets for a table
- `ReadPicklistOptions(tableName, attributeName)`: Gets options for a specific picklist

### 🔐 **Security and Privileges**

- `ReadSecurityRoles()`: Gets all security roles
- `ReadEntityPrivileges(tableName)`: Gets privileges for a specific table
- `ReadRolePrivileges(roleName)`: Gets privileges assigned to a security role
- `ReadFieldSecurityProfiles()`: Gets field-level security profiles

### 📱 **Forms and Views**

- `ReadEntityForms(tableName)`: Gets all forms for a table
- `ReadEntityViews(tableName)`: Gets all views for a table
- `ReadFormDetails(formId)`: Gets detailed form metadata
- `ReadViewDetails(viewId)`: Gets detailed view metadata

### ⚙️ **Process and Workflows**

- `ReadBusinessProcessFlows()`: Gets all business process flows
- `ReadWorkflows(tableName)`: Gets workflows for a table
- `ReadBusinessRules(tableName)`: Gets business rules for a table

### 🔧 **Advanced Attribute Metadata**

- `ReadAttributeDetails(tableName, attributeName)`: Gets detailed attribute metadata
- `ReadCalculatedFields(tableName)`: Gets calculated/computed fields
- `ReadRollupFields(tableName)`: Gets rollup fields
- `ReadLookupTargets(tableName, attributeName)`: Gets lookup target information

### 🏢 **Organization and Solution Info**

- `ReadOrganizationInfo()`: Gets organization/environment details
- `ReadLanguages()`: Gets installed languages
- `ReadCurrencies()`: Gets available currencies
- `ReadTimeZones()`: Gets available time zones
- `ReadSolutions()`: Gets all solutions (managed and unmanaged)
- `ReadSolutionDetails(solutionName)`: Gets detailed solution information
- `ReadPublishers()`: Gets all solution publishers
- `ReadSolutionComponents(solutionName)`: Gets components for a specific solution

### 🔑 **Keys and Indexes**

- `ReadEntityKeys(tableName)`: Gets all keys (alternate keys) for a table
- `ReadKeyDetails(tableName, keyName)`: Gets detailed information about a specific key
- `ReadIndexes(tableName)`: Gets all indexes for a table

### 🔌 **Plugins and Custom APIs**

- `ReadPluginAssemblies()`: Gets all plugin assemblies
- `ReadPlugins(assemblyName)`: Gets plugins in a specific assembly
- `ReadPluginSteps(pluginName)`: Gets plugin steps for a specific plugin
- `ReadCustomApis()`: Gets all custom APIs
- `ReadCustomApiDetails(apiName)`: Gets detailed information about a custom API
- `ReadCustomActions()`: Gets all custom actions

## Connection String Format

The server accepts Dataverse connection strings with the following format:

```txt
AuthType=OAuth;Url=https://yourorg.crm4.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/
```

For more connection string options, see the [Microsoft Power Platform Dataverse documentation](https://docs.microsoft.com/en-us/powerapps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect).

## Column Creation Tools

The dataverse-metadata-mcp server includes comprehensive column creation tools that allow you to create new attributes in Dataverse tables with full validation and type checking. These tools enforce Dataverse naming conventions and provide detailed error messages.

### Supported Column Types

- **Text**: Single-line text with format options (Email, URL, Phone, etc.)
- **Integer**: Whole numbers with range validation and display formats
- **Decimal**: Decimal numbers with configurable precision
- **Currency**: Money fields with proper currency formatting
- **DateTime**: Date and time with behavior options (User Local, Time Zone Independent, etc.)
- **Boolean**: Yes/No fields with customizable labels
- **Multiline Text**: Long text fields with configurable length
- **Lookup**: References to other tables with automatic relationship creation
- **Choice (Local)**: Single-select choice fields with custom options
- **Multi-Select Choice (Local)**: Multi-select choice fields with custom options
- **Choice (Global)**: Single-select choice fields using existing global choices
- **Multi-Select Choice (Global)**: Multi-select choice fields using existing global choices
- **File/Image**: File storage columns for images and documents
- **Floating Point**: Double-precision floating point numbers
- **Big Integer**: Large integer values beyond standard integer range
- **Customer**: Special lookup columns that can reference Account or Contact tables

### Key Features

- **Comprehensive Validation**: Input validation, type checking, and business rule enforcement
- **Publisher Prefix Requirement**: Column and choice names must include valid publisher prefixes (e.g., 'new_columnname', 'cr123_choicename')
- **Enterprise-Ready Configuration**: Audit settings, advanced find options, and proper permissions
- **Detailed Error Messages**: Clear feedback for validation failures and constraint violations
- **Type-Specific Options**: Specialized parameters for each data type (precision, formats, ranges, etc.)

For detailed documentation on column creation tools, including examples and best practices, see [Column Creation Tools Documentation](docs/column-creation-tools.md).

## Reusing the Tools

The `DataverseMetadataMcp.Tools` library can be integrated into other MCP servers. See the [Tools README](DataverseMetadataMcp.Tools/README.md) for detailed integration instructions.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `dotnet test`
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
