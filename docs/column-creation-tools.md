# Column Creation and Deletion Tools

This document describes the comprehensive column creation and deletion functionality added to the Dataverse MCP tools.

## Overview

The `DataverseMetadataTool.CreateColumns.cs` file contains tools for creating various types of columns (attributes) in Dataverse tables, as well as a new tool for deleting columns with comprehensive error reporting.

## Column Creation Methods

The following column creation methods are available:

### Text Columns

- `CreateTextColumn` - Creates single-line text columns with various formats (Email, Text, TextArea, Url, TickerSymbol, PhoneNumber, RichText)
- `CreateMultilineTextColumn` - Creates multi-line text columns for longer text content

### Numeric Columns

- `CreateIntegerColumn` - Creates whole number columns with min/max values and formatting options
- `CreateDecimalColumn` - Creates decimal number columns with precision control
- `CreateCurrencyColumn` - Creates currency columns with precision and currency handling
- `CreateFloatingPointColumn` - Creates floating-point number columns
- `CreateBigIntegerColumn` - Creates big integer columns for very large numbers

### Date and Time Columns

- `CreateDateTimeColumn` - Creates date/time columns with format and behavior options

### Boolean Columns

- `CreateBooleanColumn` - Creates Yes/No columns with custom labels

### Choice Columns

- `CreateChoiceColumn` - Creates single-choice picklist columns
- `CreateMultiSelectChoiceColumn` - Creates multi-select choice columns
- `CreateGlobalChoiceColumn` - Creates columns using global choice sets
- `CreateGlobalMultiSelectChoiceColumn` - Creates multi-select columns using global choice sets

### Relationship Columns

- `CreateLookupColumn` - Creates lookup (relationship) columns to other tables
- `CreateCustomerColumn` - Creates customer lookup columns (Account/Contact)

### File Columns

- `CreateFileColumn` - Creates file attachment columns

## Column Deletion Method

### DeleteColumn

The `DeleteColumn` method provides comprehensive column deletion functionality with detailed error reporting:

**Features:**

- Validates column existence before deletion
- Provides detailed error messages for dependency issues
- Offers troubleshooting guidance when deletion fails
- Returns comprehensive information about why deletion might fail

**Common reasons for deletion failure:**

- Column is used in forms or views
- Column is referenced in business rules or workflows
- Column is used in calculated or rollup fields
- Column is referenced in custom code or plugins
- Active relationships exist (for lookup columns)
- Insufficient permissions

**Troubleshooting tips provided:**

- Remove the column from all forms and views before deletion
- Check and remove any business rules that reference the column
- Verify no workflows or Power Automate flows use the column
- Ensure no calculated fields or rollup fields reference the column
- For lookup columns, check for active relationships
- Verify sufficient permissions to delete the column

## Comprehensive Testing

The `DataverseMetadataToolCreateColumnsTests.cs` file provides extensive test coverage for all column creation and deletion methods:

### Test Coverage

- **Text Column Tests** - Single-line and multi-line text creation
- **Numeric Column Tests** - Integer, decimal, currency, floating-point, and big integer creation
- **DateTime Column Tests** - Date/time column creation with various formats
- **Boolean Column Tests** - Yes/No column creation with custom labels
- **Choice Column Tests** - Single and multi-select choice column creation
- **File Column Tests** - File attachment column creation
- **Deletion Tests** - Column deletion success and error scenarios
- **Entity Coverage** - Tests on both Account and Contact entities
- **Error Handling Tests** - Invalid input and non-existent table scenarios

### Test Features

- **Automatic Cleanup** - All created test columns are automatically deleted after test completion
- **Unique Naming** - Test columns use timestamp-based unique names to avoid conflicts
- **Comprehensive Validation** - Tests verify both API success and actual column existence in Dataverse
- **Error Analysis** - Failed tests provide detailed error information for troubleshooting

### Running the Tests

To run the column creation and deletion tests:

```powershell
# Run all column tests
dotnet test DataverseMetadataMcp.Tools.Tests/DataverseMetadataMcp.Tools.Tests.csproj --filter "FullyQualifiedName~DataverseMetadataToolCreateColumnsTests" -v normal

# Run specific test
dotnet test DataverseMetadataMcp.Tools.Tests/DataverseMetadataMcp.Tools.Tests.csproj --filter "FullyQualifiedName~CreateTextColumn_ValidInput_CreatesColumnSuccessfully" -v normal
```

### Test Prerequisites

- Valid Dataverse connection string in environment variables or user secrets
- Sufficient permissions to create and delete columns on Account and Contact entities
- The test account should have System Administrator or System Customizer role

## Usage Examples

### Creating a Text Column

```csharp
var result = await DataverseMetadataTool.CreateTextColumn(
    tableName: "account",
    columnName: "new_customfield",
    displayName: "Custom Field",
    description: "A custom text field for testing",
    maxLength: 255,
    isRequired: false,
    format: "Text"
);
```

### Creating a Choice Column

```csharp
var choices = new[]
{
    new { Value = 100000000, Label = "Option 1", Description = "First option" },
    new { Value = 100000001, Label = "Option 2", Description = "Second option" }
};

var result = await DataverseMetadataTool.CreateChoiceColumn(
    tableName: "account",
    columnName: "new_status",
    displayName: "Status",
    description: "Account status field",
    choicesJson: JsonSerializer.Serialize(choices),
    defaultValue: 100000000,
    isRequired: false
);
```

### Deleting a Column

```csharp
var result = await DataverseMetadataTool.DeleteColumn(
    tableName: "account",
    columnName: "new_customfield"
);
```

## Error Handling

All methods return JSON responses with the following structure:

**Success Response:**

```json
{
  "Success": true,
  "Message": "Column Created Successfully",
  "Data": {
    "TableName": "account",
    "LogicalName": "new_customfield",
    "SchemaName": "new_customfield",
    "DisplayName": "Custom Field",
    "Description": "A custom text field for testing"
  },
  "Timestamp": "2025-01-13T12:00:00.000Z"
}
```

**Error Response:**

```json
{
  "Success": false,
  "Message": "Failed to Delete Column",
  "Errors": [
    "Failed to delete column 'new_customfield' from table 'account': Column has dependencies",
    "This column has dependencies that prevent deletion. Common dependencies include:",
    "- Forms or views that display this column"
  ],
  "TableName": "account",
  "ColumnName": "new_customfield",
  "Timestamp": "2025-01-13T12:00:00.000Z",
  "TroubleshootingTips": [
    "Remove the column from all forms and views before deletion",
    "Check and remove any business rules that reference this column"
  ]
}
```

## Best Practices

1. **Column Naming**: Always use proper publisher prefixes (e.g., `new_`, `cr123_`, or your custom prefix)
2. **Testing**: Always test column creation and deletion in a development environment first
3. **Dependencies**: Remove all dependencies (forms, views, workflows) before attempting to delete columns
4. **Permissions**: Ensure appropriate permissions for column management operations
5. **Backup**: Consider backing up your solution before making bulk column changes

## Security Considerations

- Column creation and deletion require System Administrator or System Customizer privileges
- Always validate input parameters to prevent injection attacks
- Consider implementing additional authorization checks for production environments
- Log all column creation and deletion operations for audit purposes

## Limitations

- Some column types (like calculated fields) may have additional restrictions
- System columns cannot be deleted
- Managed solution columns have restrictions based on solution publisher
- File columns have size limitations (maximum 30,720 KB in Dataverse)
- Custom column names must follow Dataverse naming conventions

```json
{
  "tableName": "contact",
  "columnName": "new_employee_id",
  "displayName": "Employee ID",
  "description": "Unique identifier for employees in the HR system",
  "maxLength": 50,
  "isRequired": true,
  "format": "Text"
}
```

### 2. Integer Column (`CreateIntegerColumn`)

Creates a whole number column with range validation.

**Parameters:**

- `tableName` (required): Logical name of the target table
- `columnName` (required): Column name (will be validated and formatted)
- `displayName` (required): User-friendly display name
- `description` (required): Purpose and usage description
- `minValue` (optional): Minimum allowed value (default: -2147483648)
- `maxValue` (optional): Maximum allowed value (default: 2147483647)
- `isRequired` (optional): Whether column is mandatory (default: false)
- `format` (optional): Display format - None, Duration, TimeZone, Language, Locale (default: None)

### 3. Decimal Column (`CreateDecimalColumn`)

Creates a decimal number column with configurable precision.

**Parameters:**

- `precision` (optional): Number of decimal places (0-10, default: 2)
- `minValue` (optional): Minimum value (default: -100000000000)
- `maxValue` (optional): Maximum value (default: 100000000000)

### 4. Currency Column (`CreateCurrencyColumn`)

Creates a currency column with proper formatting.

**Parameters:**

- `precision` (optional): Decimal places for currency (0-4, default: 2)
- `minValue` (optional): Minimum currency value
- `maxValue` (optional): Maximum currency value

### 5. DateTime Column (`CreateDateTimeColumn`)

Creates a date and time column with behavior options.

**Parameters:**

- `format` (optional): DateOnly or DateAndTime (default: DateAndTime)
- `behavior` (optional): UserLocal, DateOnly, TimeZoneIndependent (default: UserLocal)

### 6. Boolean Column (`CreateBooleanColumn`)

Creates a yes/no column with customizable labels.

**Parameters:**

- `trueLabel` (optional): Label for true value (default: "Yes")
- `falseLabel` (optional): Label for false value (default: "No")
- `defaultValue` (optional): Default boolean value (default: false)

### 7. Multiline Text Column (`CreateMultilineTextColumn`)

Creates a multiline text column for longer content.

**Parameters:**

- `maxLength` (optional): Maximum characters (1-1048576, default: 2000)

### 8. Lookup Column (`CreateLookupColumn`)

Creates a lookup column that references another table.

**Parameters:**

- `targetTableName` (required): Table being referenced
- `relationshipName` (optional): Custom relationship name (auto-generated if not provided)

## Advanced Column Types

### 1. Choice Column (Local Options) (`CreateChoiceColumn`)

Creates a single-select dropdown with custom local options.

**Parameters:**

- `choiceOptions` (required): JSON array of options: `[{"value": 1, "label": "Option 1"}]`
- `defaultValue` (optional): Default choice value

**Example:**

```json
{
  "tableName": "account",
  "columnName": "new_industry_type",
  "displayName": "Industry Type",
  "description": "Primary industry classification for the account",
  "choiceOptions": "[{\"value\": 1, \"label\": \"Technology\"}, {\"value\": 2, \"label\": \"Healthcare\"}, {\"value\": 3, \"label\": \"Finance\"}]",
  "defaultValue": 1,
  "isRequired": true
}
```

### 2. Multi-Select Choice Column (Local Options) (`CreateMultiSelectChoiceColumn`)

Creates a multi-select dropdown allowing multiple choices with local options.

**Parameters:**

- `choiceOptions` (required): JSON array of options (same format as Choice Column)

### 3. Choice Column (Global Options) (`CreateGlobalChoiceColumn`)

Creates a single-select dropdown using an existing global choice (option set).

**Parameters:**

- `globalChoiceName` (required): Name of the existing global choice to use

**Example:**

```json
{
  "tableName": "account",
  "columnName": "new_priority",
  "displayName": "Account Priority",
  "description": "Priority level for this account using global priority choices",
  "globalChoiceName": "new_priority_levels",
  "isRequired": false
}
```

### 4. Multi-Select Choice Column (Global Options) (`CreateGlobalMultiSelectChoiceColumn`)

Creates a multi-select dropdown using an existing global choice (option set).

**Parameters:**

- `globalChoiceName` (required): Name of the existing global choice to use

### 5. File Column (`CreateFileColumn`)

Creates a column for storing file attachments (images and documents).

**Parameters:**

- Standard column parameters (tableName, columnName, displayName, description, isRequired)

**Example:**

```json
{
  "tableName": "contact",
  "columnName": "new_profile_photo",
  "displayName": "Profile Photo",
  "description": "Employee profile photo for identification",
  "isRequired": false
}
```

### 6. Floating Point Column (`CreateFloatingPointColumn`)

Creates a high-precision numeric column for decimal calculations.

**Parameters:**

- `minValue` (optional): Minimum value (default: -100000000000)
- `maxValue` (optional): Maximum value (default: 100000000000)
- `precision` (optional): Number of decimal places (0-5, default: 2)

### 7. Big Integer Column (`CreateBigIntegerColumn`)

Creates a column for very large integer values (64-bit range).

**Parameters:**

- Standard column parameters with no additional constraints
- Supports values from -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807

### 8. Customer Column (`CreateCustomerColumn`)

Creates a special lookup column that can reference either Account or Contact tables.

**Parameters:**

- Standard column parameters (tableName, columnName, displayName, description, isRequired)

**Example:**

```json
{
  "tableName": "opportunity",
  "columnName": "new_decision_maker",
  "displayName": "Decision Maker",
  "description": "The primary decision maker for this opportunity (can be Account or Contact)",
  "isRequired": true
}
```

## Naming Convention Rules

### Input Validation

- Column names must start with a letter
- Only letters, numbers, and underscores allowed
- Maximum 100 characters
- Reserved words are blocked (id, createdby, createdon, etc.)

### Automatic Transformations

- Converts to lowercase
- Replaces spaces and special characters with underscores
- Removes consecutive underscores
- Adds custom prefix if none exists (new*, cr*, abc\_)
- Schema name is the same as logical name (lowercase)

### Example Transformations

- `"Employee ID"` → `"new_employee_id"` (logical and schema)
- `"Contact__Phone"` → `"new_contact_phone"` (logical and schema)
- `"123numbers"` → `"col_123numbers"` (logical and schema)

## Response Format

All tools return a JSON response with the following structure:

### Success Response

```json
{
  "Success": true,
  "Message": "Column Created Successfully",
  "Data": {
    "TableName": "contact",
    "LogicalName": "new_employee_id",
    "SchemaName": "new_employee_id",
    "DisplayName": "Employee ID",
    "Description": "Employee identifier",
    "IsRequired": true
    // Additional type-specific properties
  },
  "Timestamp": "2025-06-13T10:30:00.000Z"
}
```

### Error Response

```json
{
  "Success": false,
  "Message": "Input Validation Failed",
  "Errors": ["Column name is required.", "Display name is required."],
  "Timestamp": "2025-06-13T10:30:00.000Z"
}
```

## Best Practices

### Column Design

1. Use descriptive display names and comprehensive descriptions
2. Set appropriate data type constraints (length, range, precision)
3. Consider making critical business fields required
4. Use choice columns for standardized values

### Naming Conventions

1. Use clear, business-friendly column names
2. Avoid abbreviations when possible
3. Use consistent naming patterns across your solution
4. Let the tools handle technical formatting

### Data Integrity

1. Set appropriate minimum/maximum values for numeric fields
2. Use lookup columns to maintain referential integrity
3. Consider default values for boolean and choice columns
4. Document column purposes thoroughly

### Performance Considerations

1. Limit text field lengths to actual requirements
2. Use appropriate numeric types (integer vs. decimal vs. currency)
3. Consider indexing needs when designing columns
4. Image and file columns should be used judiciously

## Error Handling

The tools provide comprehensive error handling:

- **Input Validation**: Checks for required fields and format compliance
- **Type-Specific Validation**: Range checks, format validation, option validation
- **Dataverse Constraints**: Reserved word detection, naming convention enforcement
- **System Errors**: Connection issues, permission problems, service errors

All errors include detailed messages to help identify and resolve issues quickly.

## Integration with MCP

These tools are designed to work seamlessly with Model Context Protocol clients:

1. **Tool Discovery**: All tools are automatically registered and discoverable
2. **Parameter Validation**: Built-in validation reduces round-trips
3. **Detailed Descriptions**: Rich metadata helps with tool selection
4. **Consistent Interface**: Uniform parameter patterns across all tools
5. **Comprehensive Responses**: Detailed success and error information

This implementation provides a robust, production-ready solution for creating Dataverse columns through MCP, with enterprise-grade validation, error handling, and documentation.

## Global Choice (Option Set) Creation

### Creating Global Choices (`CreateGlobalChoice`)

Creates a new global choice (option set) in Dataverse with custom options and comprehensive validation.

**Parameters:**

- `choiceName` (required): Logical name with publisher prefix (e.g., 'new_priority_level')
- `displayName` (required): User-friendly display name
- `description` (required): Purpose and usage description
- `choiceOptions` (required): JSON array of choice options with value and label

**Option Format:**

```json
[
  { "value": 1, "label": "Low" },
  { "value": 2, "label": "Medium" },
  { "value": 3, "label": "High" },
  { "value": 4, "label": "Critical" }
]
```

**Example Usage:**

```json
{
  "choiceName": "new_priority_level",
  "displayName": "Priority Level",
  "description": "Priority levels for tasks and incidents",
  "choiceOptions": "[{\"value\": 1, \"label\": \"Low\"}, {\"value\": 2, \"label\": \"Medium\"}, {\"value\": 3, \"label\": \"High\"}, {\"value\": 4, \"label\": \"Critical\"}]"
}
```

**Key Features:**

- Publisher prefix validation (required)
- Duplicate value detection
- Label validation
- Reserved prefix protection
- Comprehensive error reporting
