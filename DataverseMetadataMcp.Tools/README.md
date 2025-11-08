# DataverseMetadataMcp.Tools

A comprehensive reusable library for Power Platform MCP (Model Context Protocol) tools that provides extensive Dataverse metadata access capabilities.

## Overview

This library contains a complete suite of MCP tools for retrieving Dataverse metadata information, designed to be used in any MCP server implementation. The tools can be integrated into different server projects, making the Power Platform functionality reusable across multiple applications.

## Features

### 🏗️ **Complete Dataverse Metadata Coverage**

**Table and Column Metadata:**

- `ReadTables()`, `ReadTable(tableName)`, `ReadColumns(tableName)`

**Column Creation Tools:**

- `CreateTextColumn()`, `CreateIntegerColumn()`, `CreateDecimalColumn()`, `CreateCurrencyColumn()`, `CreateDateTimeColumn()`, `CreateBooleanColumn()`, `CreateMultilineTextColumn()`, `CreateLookupColumn()` - Basic data types
- `CreateChoiceColumn()`, `CreateMultiSelectChoiceColumn()` - Choice columns with local options
- `CreateGlobalChoiceColumn()`, `CreateGlobalMultiSelectChoiceColumn()` - Choice columns using existing global choices
- `CreateFileColumn()`, `CreateFloatingPointColumn()`, `CreateBigIntegerColumn()`, `CreateCustomerColumn()` - Advanced data types
- All methods require publisher prefix in column names (e.g., 'new_columnname')

### 🔧 **Column Creation Features**

- **Type Safety & Validation**: Comprehensive input validation with type-specific constraints
- **Naming Convention Enforcement**: Automatic formatting to Dataverse standards with prefix validation
- **Rich Data Types**: Support for all major Dataverse column types including choices, files, and specialized lookups
- **Global Choice Integration**: Create choice columns using existing global choices or create new local choices
- **Relationship Creation**: Automatic lookup relationships with proper cascade configuration
- **Enterprise-Ready**: Audit enablement, advanced find configuration, and proper permissions

**Relationship Metadata:**

- `ReadEntityRelationships(tableName)`, `ReadOneToManyRelationships(tableName)`, `ReadManyToOneRelationships(tableName)`, `ReadManyToManyRelationships(tableName)`

**Choice/Picklist Metadata:**

- `CreateGlobalChoice(choiceName, displayName, description, choiceOptions)` - Creates a new global choice (option set) with validation
- `ReadGlobalOptionSets()`, `ReadGlobalOptionSet(optionSetName)`, `ReadLocalOptionSets(tableName)`, `ReadPicklistOptions(tableName, attributeName)`

**Security and Privileges:**

- `ReadSecurityRoles()`, `ReadEntityPrivileges(tableName)`, `ReadRolePrivileges(roleName)`, `ReadFieldSecurityProfiles()`

**Forms and Views:**

- `ReadEntityForms(tableName)`, `ReadEntityViews(tableName)`, `ReadFormDetails(formId)`, `ReadViewDetails(viewId)`

**Process and Workflows:**

- `ReadBusinessProcessFlows()`, `ReadWorkflows(tableName)`, `ReadBusinessRules(tableName)`

**Advanced Attribute Metadata:**

- `ReadAttributeDetails(tableName, attributeName)`, `ReadCalculatedFields(tableName)`, `ReadRollupFields(tableName)`, `ReadLookupTargets(tableName, attributeName)`

**Organization and Solution Info:**

- `ReadOrganizationInfo()`, `ReadLanguages()`, `ReadCurrencies()`, `ReadTimeZones()`, `ReadSolutions()`, `ReadSolutionDetails(solutionName)`, `ReadPublishers()`, `ReadSolutionComponents(solutionName)`

**Keys and Indexes:**

- `ReadEntityKeys(tableName)`, `ReadKeyDetails(tableName, keyName)`, `ReadIndexes(tableName)`

**Plugins and Custom APIs:**

- `ReadPluginAssemblies()`, `ReadPlugins(assemblyName)`, `ReadPluginSteps(pluginName)`, `ReadCustomApis()`, `ReadCustomApiDetails(apiName)`, `ReadCustomActions()`

### 🔧 **Infrastructure Features**

- **Connection Management**: Handles Dataverse authentication and connection validation
- **Configuration Helper**: Provides static access to connection services from MCP tools
- **Modular Design**: Split across multiple partial classes for maintainability
- **Enterprise Ready**: Comprehensive security, solution, and organizational metadata access

## Usage

### In your MCP Server project

1. Add a reference to `DataverseMetadataMcp.Tools`
2. Register the services in your `Program.cs`:

   ```csharp
   using DataverseMetadataMcp.Tools;
   using DataverseMetadataMcp.Tools.Configuration;

   // Register Power Platform MCP tools and dependencies
   builder.Services.AddPowerPlatformMcpTools();

   // Register MCP server and scan for tools in the Tools assembly
   builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithToolsFromAssembly(typeof(PowerPlatformMcpToolsInitializer).Assembly);

   // After building the host, initialize the tools
   var host = builder.Build();
   PowerPlatformMcpToolsInitializer.Initialize(host.Services.GetRequiredService<DataverseConnectionService>());
   ```

3. Provide a Dataverse connection string through configuration:
   - Command line: `--connection-string "your-connection-string"`
   - Configuration file: `"ConnectionString": "your-connection-string"`

### Connection String Format

The library supports OAuth-based authentication. Example connection string:

```text
AuthType=OAuth;Url=https://yourorg.crm.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/
```

## Project Structure

```text
DataverseMetadataMcp.Tools/
├── Configuration/
│   ├── ConfigurationHelper.cs              # Static helper for accessing services
│   └── DataverseConnectionService.cs       # Manages Dataverse connections
├── Tools/
│   ├── DataverseMetadataTool.TableAndColumn.cs          # Table and column metadata
│   ├── DataverseMetadataTool.CreateColumns.cs           # Column creation tools
│   ├── DataverseMetadataTool.Choices.cs                 # Choice/picklist metadata
│   ├── DataverseMetadataTool.Security.cs                # Security and privileges
│   ├── DataverseMetadataTool.FormsAndViews.cs          # Forms and views metadata
│   ├── DataverseMetadataTool.ProcessWorkflowsMethods.cs # Process and workflow metadata
│   ├── DataverseMetadataTool.AdvancedAttributesMetadataMethods.cs # Advanced attributes
│   ├── DataverseMetadataTool.OrganizationSolution.cs   # Organization and solution info
│   ├── DataverseMetadataTool.KeysIndexes.cs            # Keys and indexes metadata
│   └── DataverseMetadataTool.PluginsCustomApis.cs      # Plugins and custom APIs metadata
├── ServiceCollectionExtensions.cs          # DI registration extensions
└── PowerPlatformMcpToolsInitializer.cs    # Initialization helper
```

## Dependencies

- Microsoft.PowerPlatform.Dataverse.Client
- ModelContextProtocol
- Microsoft.Extensions.Hosting.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Microsoft.Extensions.Configuration.Abstractions

## Example: Using in a Different Server

You can easily integrate these tools into any MCP server:

```csharp
// In a different server project
builder.Services.AddPowerPlatformMcpTools();

// Add your own tools as well
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly() // Your own tools
    .WithToolsFromAssembly(typeof(PowerPlatformMcpToolsInitializer).Assembly); // PP tools
```

## Benefits

This separation allows you to:

- **Reuse Power Platform functionality** across multiple servers
- **Maintain and update the tools independently** from server implementations
- **Combine with other MCP tool libraries** for comprehensive solutions
- **Create specialized servers** for different scenarios
- **Access comprehensive metadata** for enterprise Power Platform development
- **Build powerful integrations** with complete organizational context

## Version 2.3 Features

This release includes:

- **Enhanced Column Creation Tools**: 16+ comprehensive tools for creating different types of columns (attributes) in Dataverse tables with advanced validation and error reporting
- **Global Choice Creation**: Create new global choices (option sets) with custom options and validation
- **Comprehensive Validation**: Type checking, range validation, format validation, naming convention enforcement, and detailed error reporting
- **Enterprise-Grade**: Proper audit configuration, advanced find settings, permission management, and enhanced security features
- **35+ metadata tools** covering all aspects of Dataverse (from version 2.0)
- **Complete relationship analysis** with cascade configuration details
- **Security model introspection** with roles, privileges, and field security
- **Solution lifecycle management** with component analysis
- **Organizational insights** including currencies, languages, and time zones
- **Keys and indexes metadata** for performance and data integrity analysis
- **Plugin and custom API discovery** for extensibility insights
- **Process and workflow discovery** for automation analysis
- **Advanced attribute metadata** for calculated fields, rollups, and lookups
