using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.PowerPlatform.Dataverse.Client;
using ModelContextProtocol.Server;
using DataverseMetadataMcp.Tools.Configuration;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataverseMetadataMcp.Tools.Tools;

/// <summary>
/// Represents a choice option for Dataverse choice columns
/// </summary>
public class ChoiceOption
{
    public int Value { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Validation result structure
/// </summary>
internal class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

/// <summary>
/// MCP tools for creating new columns (attributes) in Dataverse tables
/// </summary>
public static partial class DataverseMetadataTool
{
    #region Column Creation Methods    /// <summary>
    /// Creates a new text (string) column in a Dataverse table
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="maxLength">Maximum length of text (1-4000, default: 100)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <param name="format">Text format: Email, Text, TextArea, Url, TickerSymbol, PhoneNumber, or RichText (default: Text)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new text (string) column in a Dataverse table with comprehensive validation. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateTextColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int maxLength = 100,
        bool isRequired = false,
        string format = "Text")
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate max length
            if (maxLength < 1 || maxLength > 4000)
                return CreateErrorResponse("Invalid Max Length", new[] { "Max length must be between 1 and 4000 characters." });

            // Validate format
            var validFormats = new[] { "Email", "Text", "TextArea", "Url", "TickerSymbol", "PhoneNumber", "RichText" };
            if (!validFormats.Contains(format))
                return CreateErrorResponse("Invalid Format", new[] { $"Format must be one of: {string.Join(", ", validFormats)}" }); var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Create the attribute metadata
            var stringAttribute = new StringAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                MaxLength = maxLength,
                Format = Enum.Parse<StringFormat>(format),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = stringAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Text Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = stringAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                MaxLength = maxLength,
                Format = format,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Text Column", new[] { ex.Message });
        }
    }    /// <summary>
         /// Creates a new whole number (integer) column in a Dataverse table
         /// </summary>
         /// <param name="tableName">The logical name of the table to add the column to</param>
         /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
         /// <param name="displayName">The display name for the column</param>
         /// <param name="description">Description of the column's purpose</param>
         /// <param name="minValue">Minimum value allowed (default: -2147483648)</param>
         /// <param name="maxValue">Maximum value allowed (default: 2147483647)</param>
         /// <param name="isRequired">Whether the column is required (default: false)</param>
         /// <param name="format">Integer format: None, Duration, TimeZone, Language, or Locale (default: None)</param>
         /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new whole number (integer) column in a Dataverse table with validation and proper formatting options. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateIntegerColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int minValue = -2147483648,
        int maxValue = 2147483647,
        bool isRequired = false,
        string format = "None")
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate min/max values
            if (minValue > maxValue)
                return CreateErrorResponse("Invalid Range", new[] { "Minimum value cannot be greater than maximum value." });

            // Validate format
            var validFormats = new[] { "None", "Duration", "TimeZone", "Language", "Locale" };
            if (!validFormats.Contains(format)) return CreateErrorResponse("Invalid Format", new[] { $"Format must be one of: {string.Join(", ", validFormats)}" });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var integerAttribute = new IntegerAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                MinValue = minValue,
                MaxValue = maxValue,
                Format = Enum.Parse<IntegerFormat>(format),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = integerAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Integer Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = integerAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                MinValue = minValue,
                MaxValue = maxValue,
                Format = format,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Integer Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new decimal number column in a Dataverse table
    /// </summary>    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="precision">Number of decimal places (0-10, default: 2)</param>
    /// <param name="minValue">Minimum value allowed (default: -100000000000)</param>
    /// <param name="maxValue">Maximum value allowed (default: 100000000000)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new decimal number column in a Dataverse table with configurable precision and range validation. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateDecimalColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int precision = 2,
        decimal minValue = -100000000000,
        decimal maxValue = 100000000000,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate precision
            if (precision < 0 || precision > 10)
                return CreateErrorResponse("Invalid Precision", new[] { "Precision must be between 0 and 10." });

            // Validate min/max values
            if (minValue > maxValue)
                return CreateErrorResponse("Invalid Range", new[] { "Minimum value cannot be greater than maximum value." }); var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var decimalAttribute = new DecimalAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                Precision = precision,
                MinValue = minValue,
                MaxValue = maxValue,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = decimalAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Decimal Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = decimalAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                Precision = precision,
                MinValue = minValue,
                MaxValue = maxValue,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Decimal Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new currency (money) column in a Dataverse table
    /// </summary>    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="precision">Number of decimal places (0-4, default: 2)</param>
    /// <param name="minValue">Minimum value allowed (default: -922337203685477)</param>
    /// <param name="maxValue">Maximum value allowed (default: 922337203685477)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new currency (money) column in a Dataverse table with proper currency formatting and precision. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateCurrencyColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int precision = 2,
        double minValue = -922337203685477,
        double maxValue = 922337203685477,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate precision
            if (precision < 0 || precision > 4)
                return CreateErrorResponse("Invalid Precision", new[] { "Precision for currency must be between 0 and 4." });

            // Validate min/max values
            if (minValue > maxValue)
                return CreateErrorResponse("Invalid Range", new[] { "Minimum value cannot be greater than maximum value." }); var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var moneyAttribute = new MoneyAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                Precision = precision,
                MinValue = minValue,
                MaxValue = maxValue,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = moneyAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Currency Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = moneyAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                Precision = precision,
                MinValue = minValue,
                MaxValue = maxValue,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Currency Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new date and time column in a Dataverse table
    /// </summary>    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="format">Date format: DateOnly, DateAndTime (default: DateAndTime)</param>
    /// <param name="behavior">Date behavior: UserLocal, DateOnly, TimeZoneIndependent (default: UserLocal)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new date and time column in a Dataverse table with configurable format and behavior options. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateDateTimeColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string format = "DateAndTime",
        string behavior = "UserLocal",
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate format
            var validFormats = new[] { "DateOnly", "DateAndTime" };
            if (!validFormats.Contains(format))
                return CreateErrorResponse("Invalid Format", new[] { $"Format must be one of: {string.Join(", ", validFormats)}" });

            // Validate behavior
            var validBehaviors = new[] { "UserLocal", "DateOnly", "TimeZoneIndependent" };
            if (!validBehaviors.Contains(behavior))
                return CreateErrorResponse("Invalid Behavior", new[] { $"Behavior must be one of: {string.Join(", ", validBehaviors)}" }); var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Parse behavior correctly
            DateTimeBehavior? dateBehavior = behavior switch
            {
                "UserLocal" => DateTimeBehavior.UserLocal,
                "DateOnly" => DateTimeBehavior.DateOnly,
                "TimeZoneIndependent" => DateTimeBehavior.TimeZoneIndependent,
                _ => null
            };

            var dateTimeAttribute = new DateTimeAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                Format = Enum.Parse<DateTimeFormat>(format),
                DateTimeBehavior = dateBehavior,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = dateTimeAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("DateTime Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = dateTimeAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                Format = format,
                Behavior = behavior,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating DateTime Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new yes/no (boolean) column in a Dataverse table
    /// </summary>    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="trueLabel">Label for true value (default: Yes)</param>
    /// <param name="falseLabel">Label for false value (default: No)</param>
    /// <param name="defaultValue">Default value (default: false)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new yes/no (boolean) column in a Dataverse table with customizable labels and default value. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateBooleanColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string trueLabel = "Yes",
        string falseLabel = "No",
        bool defaultValue = false,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            if (string.IsNullOrWhiteSpace(trueLabel) || string.IsNullOrWhiteSpace(falseLabel))
                return CreateErrorResponse("Invalid Labels", new[] { "True and false labels cannot be empty." });
            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var booleanAttribute = new BooleanAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                OptionSet = new BooleanOptionSetMetadata(
                    new OptionMetadata(new Label(trueLabel, 1033), 1),
                    new OptionMetadata(new Label(falseLabel, 1033), 0)
                ),
                DefaultValue = defaultValue,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = booleanAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Boolean Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = booleanAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                TrueLabel = trueLabel,
                FalseLabel = falseLabel,
                DefaultValue = defaultValue,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Boolean Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new multiline text column in a Dataverse table
    /// </summary>    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="maxLength">Maximum length of text (1-1048576, default: 2000)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new multiline text column in a Dataverse table for longer text content with configurable maximum length. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateMultilineTextColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int maxLength = 2000,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate max length
            if (maxLength < 1 || maxLength > 1048576)
                return CreateErrorResponse("Invalid Max Length", new[] { "Max length must be between 1 and 1,048,576 characters." });
            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var memoAttribute = new MemoAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                MaxLength = maxLength,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = memoAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Multiline Text Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = memoAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                MaxLength = maxLength,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Multiline Text Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new lookup column in a Dataverse table
    /// </summary>    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="targetTableName">The logical name of the table this lookup references</param>
    /// <param name="relationshipName">Name for the relationship (optional, will be auto-generated if not provided)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new lookup column in a Dataverse table that references another table, establishing a many-to-one relationship. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateLookupColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string targetTableName,
        string relationshipName = "",
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            if (string.IsNullOrWhiteSpace(targetTableName))
                return CreateErrorResponse("Invalid Target Table", new[] { "Target table name is required for lookup columns." });
            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Generate relationship name if not provided
            if (string.IsNullOrWhiteSpace(relationshipName))
            {
                relationshipName = $"{targetTableName}_{tableName}_{logicalName}";
            }

            var lookupAttribute = new LookupAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };            // Get the primary key attribute name for the target table
            var primaryKeyAttribute = await GetPrimaryKeyAttributeName(serviceClient, targetTableName);
            if (string.IsNullOrEmpty(primaryKeyAttribute))
            {
                return CreateErrorResponse("Target Table Invalid", new[] {
                    $"Could not determine primary key for target table '{targetTableName}'. Please verify the table exists and is accessible."
                });
            }
            var relationship = new OneToManyRelationshipMetadata
            {
                SchemaName = relationshipName,
                ReferencedEntity = targetTableName,
                ReferencingEntity = tableName,
                ReferencedAttribute = primaryKeyAttribute // Primary key of target table
                // ReferencingAttribute is left empty - it will be auto-generated to match the lookup attribute
            };

            var request = new CreateOneToManyRequest
            {
                OneToManyRelationship = relationship,
                Lookup = lookupAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Lookup Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = lookupAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                TargetTableName = targetTableName,
                RelationshipName = relationshipName,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Lookup Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new choice (option set) column in a Dataverse table with local options
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="choiceOptions">JSON array of choice options in format: [{"value": 1, "label": "Option 1"}, {"value": 2, "label": "Option 2"}]</param>
    /// <param name="defaultValue">Default option value (optional)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new choice (option set) column in a Dataverse table with local options. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateChoiceColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string choiceOptions,
        int? defaultValue = null,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Parse and validate choice options
            List<ChoiceOption> options;
            try
            {
                options = JsonSerializer.Deserialize<List<ChoiceOption>>(choiceOptions) ?? new List<ChoiceOption>();
            }
            catch (JsonException)
            {
                return CreateErrorResponse("Invalid Choice Options", new[] { "Choice options must be valid JSON array with 'value' and 'label' properties." });
            }

            if (options.Count == 0)
                return CreateErrorResponse("No Choice Options", new[] { "At least one choice option is required." });

            if (options.Any(o => string.IsNullOrWhiteSpace(o.Label)))
                return CreateErrorResponse("Invalid Choice Options", new[] { "All choice options must have a label." });

            // Check for duplicate values
            var duplicateValues = options.GroupBy(o => o.Value).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateValues.Any())
                return CreateErrorResponse("Duplicate Values", new[] { $"Duplicate option values found: {string.Join(", ", duplicateValues)}" });

            // Validate default value if provided
            if (defaultValue.HasValue && !options.Any(o => o.Value == defaultValue.Value))
                return CreateErrorResponse("Invalid Default Value", new[] { $"Default value {defaultValue} is not in the list of available options." });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Create option metadata
            var optionMetadata = options.Select(o => new OptionMetadata(new Label(o.Label, 1033), o.Value)).ToList();

            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = false,
                OptionSetType = OptionSetType.Picklist,
                Options = { }
            };

            foreach (var option in optionMetadata)
            {
                optionSetMetadata.Options.Add(option);
            }

            var picklistAttribute = new PicklistAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                OptionSet = optionSetMetadata,
                DefaultFormValue = defaultValue,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = picklistAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Choice Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = picklistAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                Options = options,
                DefaultValue = defaultValue,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Choice Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new multi-select choice column in a Dataverse table
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="choiceOptions">JSON array of choice options in format: [{"value": 1, "label": "Option 1"}, {"value": 2, "label": "Option 2"}]</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new multi-select choice column in a Dataverse table allowing multiple option selection. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateMultiSelectChoiceColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string choiceOptions,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Parse and validate choice options
            List<ChoiceOption> options;
            try
            {
                options = JsonSerializer.Deserialize<List<ChoiceOption>>(choiceOptions) ?? new List<ChoiceOption>();
            }
            catch (JsonException)
            {
                return CreateErrorResponse("Invalid Choice Options", new[] { "Choice options must be valid JSON array with 'value' and 'label' properties." });
            }

            if (options.Count == 0)
                return CreateErrorResponse("No Choice Options", new[] { "At least one choice option is required." });

            if (options.Any(o => string.IsNullOrWhiteSpace(o.Label)))
                return CreateErrorResponse("Invalid Choice Options", new[] { "All choice options must have a label." });

            // Check for duplicate values
            var duplicateValues = options.GroupBy(o => o.Value).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateValues.Any())
                return CreateErrorResponse("Duplicate Values", new[] { $"Duplicate option values found: {string.Join(", ", duplicateValues)}" });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Create option metadata
            var optionMetadata = options.Select(o => new OptionMetadata(new Label(o.Label, 1033), o.Value)).ToList();

            var optionSetMetadata = new OptionSetMetadata
            {
                IsGlobal = false,
                OptionSetType = OptionSetType.Picklist,
                Options = { }
            };

            foreach (var option in optionMetadata)
            {
                optionSetMetadata.Options.Add(option);
            }

            var multiSelectAttribute = new MultiSelectPicklistAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                OptionSet = optionSetMetadata,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = multiSelectAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Multi-Select Choice Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = multiSelectAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                Options = options,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Multi-Select Choice Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new choice column in a Dataverse table using an existing global choice (option set)
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="globalChoiceName">The logical name of the existing global choice (option set) to use</param>
    /// <param name="defaultValue">Default option value (optional)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new choice column in a Dataverse table using an existing global choice (option set). Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateGlobalChoiceColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string globalChoiceName,
        int? defaultValue = null,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            if (string.IsNullOrWhiteSpace(globalChoiceName))
                return CreateErrorResponse("Invalid Global Choice", new[] { "Global choice name is required." });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Validate that the global choice exists
            try
            {
                var optionSetRequest = new RetrieveOptionSetRequest
                {
                    Name = globalChoiceName,
                    RetrieveAsIfPublished = true
                };
                var optionSetResponse = (RetrieveOptionSetResponse)await serviceClient.ExecuteAsync(optionSetRequest);

                // Validate default value if provided
                if (defaultValue.HasValue && optionSetResponse.OptionSetMetadata is OptionSetMetadata globalOptionSet)
                {
                    if (!globalOptionSet.Options.Any(o => o.Value == defaultValue.Value))
                        return CreateErrorResponse("Invalid Default Value", new[] { $"Default value {defaultValue} is not in the global choice '{globalChoiceName}'." });
                }
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("Global Choice Not Found", new[] { $"Global choice '{globalChoiceName}' does not exist or is not accessible: {ex.Message}" });
            }

            var picklistAttribute = new PicklistAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                OptionSet = new OptionSetMetadata
                {
                    Name = globalChoiceName,
                    IsGlobal = true
                },
                DefaultFormValue = defaultValue,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = picklistAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Global Choice Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = picklistAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                GlobalChoiceName = globalChoiceName,
                DefaultValue = defaultValue,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Global Choice Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new multi-select choice column in a Dataverse table using an existing global choice (option set)
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="globalChoiceName">The logical name of the existing global choice (option set) to use</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new multi-select choice column in a Dataverse table using an existing global choice (option set). Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateGlobalMultiSelectChoiceColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string globalChoiceName,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            if (string.IsNullOrWhiteSpace(globalChoiceName))
                return CreateErrorResponse("Invalid Global Choice", new[] { "Global choice name is required." });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Validate that the global choice exists
            try
            {
                var optionSetRequest = new RetrieveOptionSetRequest
                {
                    Name = globalChoiceName,
                    RetrieveAsIfPublished = true
                };
                await serviceClient.ExecuteAsync(optionSetRequest);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("Global Choice Not Found", new[] { $"Global choice '{globalChoiceName}' does not exist or is not accessible: {ex.Message}" });
            }

            var multiSelectAttribute = new MultiSelectPicklistAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                OptionSet = new OptionSetMetadata
                {
                    Name = globalChoiceName,
                    IsGlobal = true
                },
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = multiSelectAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Global Multi-Select Choice Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = multiSelectAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                GlobalChoiceName = globalChoiceName,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Global Multi-Select Choice Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new file (image) column in a Dataverse table
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="maxSizeInKB">Maximum file size in KB (1-32768, default: 32768)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new file (image) column in a Dataverse table for storing image files. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateFileColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int maxSizeInKB = 32768,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate max size
            if (maxSizeInKB < 1 || maxSizeInKB > 32768)
                return CreateErrorResponse("Invalid Max Size", new[] { "Max size must be between 1 and 32,768 KB." });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var imageAttribute = new ImageAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                MaxSizeInKB = maxSizeInKB,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(false), // Images typically not searchable
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = imageAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("File Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = imageAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                MaxSizeInKB = maxSizeInKB,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating File Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new floating point number (double) column in a Dataverse table
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="precision">Number of decimal places (0-5, default: 2)</param>
    /// <param name="minValue">Minimum value allowed (default: -100000000000)</param>
    /// <param name="maxValue">Maximum value allowed (default: 100000000000)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new floating point number (double) column in a Dataverse table with configurable precision. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateFloatingPointColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        int precision = 2,
        double minValue = -100000000000,
        double maxValue = 100000000000,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            // Validate precision
            if (precision < 0 || precision > 5)
                return CreateErrorResponse("Invalid Precision", new[] { "Precision must be between 0 and 5." });

            // Validate min/max values
            if (minValue > maxValue)
                return CreateErrorResponse("Invalid Range", new[] { "Minimum value cannot be greater than maximum value." });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            var doubleAttribute = new DoubleAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                Precision = precision,
                MinValue = minValue,
                MaxValue = maxValue,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = doubleAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Floating Point Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = doubleAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                Precision = precision,
                MinValue = minValue,
                MaxValue = maxValue,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Floating Point Column", new[] { ex.Message });
        }
    }    /// <summary>
         /// Creates a new big integer column in a Dataverse table
         /// </summary>
         /// <param name="tableName">The logical name of the table to add the column to</param>
         /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
         /// <param name="displayName">The display name for the column</param>
         /// <param name="description">Description of the column's purpose</param>
         /// <param name="isRequired">Whether the column is required (default: false)</param>
         /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new big integer (64-bit) column in a Dataverse table for very large numbers. Uses the full 64-bit range. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateBigIntegerColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName); var bigIntAttribute = new BigIntAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true
            };

            // Note: BigInt columns in Dataverse use the full range of 64-bit integers
            // Custom min/max values are not supported for this attribute type

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = bigIntAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Big Integer Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = bigIntAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                IsRequired = isRequired,
                Range = "Uses full 64-bit integer range (-9,223,372,036,854,775,808 to 9,223,372,036,854,775,807)"
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Big Integer Column", new[] { ex.Message });
        }
    }

    /// <summary>
    /// Creates a new customer column in a Dataverse table (special lookup to Account or Contact)
    /// </summary>
    /// <param name="tableName">The logical name of the table to add the column to</param>
    /// <param name="columnName">The logical name for the new column (must include publisher prefix, e.g., 'new_columnname')</param>
    /// <param name="displayName">The display name for the column</param>
    /// <param name="description">Description of the column's purpose</param>
    /// <param name="relationshipName">Name for the relationship (optional, will be auto-generated if not provided)</param>
    /// <param name="isRequired">Whether the column is required (default: false)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new customer column in a Dataverse table that can reference either Account or Contact tables. Column name must include publisher prefix (e.g., 'new_columnname').")]
    public static async Task<string> CreateCustomerColumn(
        string tableName,
        string columnName,
        string displayName,
        string description,
        string relationshipName = "",
        bool isRequired = false)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateCommonInputs(tableName, columnName, displayName, description);
            if (!validationResult.IsValid)
                return CreateErrorResponse("Input Validation Failed", validationResult.Errors);

            var serviceClient = ConfigurationHelper.GetServiceClient();
            var logicalName = ValidateAndCleanColumnName(columnName);

            // Generate relationship name if not provided
            if (string.IsNullOrWhiteSpace(relationshipName))
            {
                relationshipName = $"customer_{tableName}_{logicalName}";
            }

            var customerAttribute = new LookupAttributeMetadata
            {
                LogicalName = logicalName,
                SchemaName = ConvertToSchemaName(logicalName),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(
                    isRequired ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                IsAuditEnabled = new BooleanManagedProperty(true),
                IsValidForAdvancedFind = new BooleanManagedProperty(true),
                IsValidForCreate = true,
                IsValidForUpdate = true,
                // Customer attributes can reference both Account and Contact
                Targets = new[] { "account", "contact" }
            };

            var request = new CreateAttributeRequest
            {
                EntityName = tableName,
                Attribute = customerAttribute
            };

            await serviceClient.ExecuteAsync(request);

            return CreateSuccessResponse("Customer Column Created Successfully", new
            {
                TableName = tableName,
                LogicalName = logicalName,
                SchemaName = customerAttribute.SchemaName,
                DisplayName = displayName,
                Description = description,
                TargetTables = new[] { "account", "contact" },
                RelationshipName = relationshipName,
                IsRequired = isRequired
            });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("Error Creating Customer Column", new[] { ex.Message });
        }
    }

    #endregion

    #region Helper Methods/// <summary>
    /// Validates common inputs for column creation
    /// </summary>
    private static ValidationResult ValidateCommonInputs(string tableName, string columnName, string displayName, string description)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(tableName))
            errors.Add("Table name is required.");

        if (string.IsNullOrWhiteSpace(columnName))
            errors.Add("Column name is required.");

        if (string.IsNullOrWhiteSpace(displayName))
            errors.Add("Display name is required.");

        if (string.IsNullOrWhiteSpace(description))
            errors.Add("Description is required.");

        // Validate column name format
        if (!string.IsNullOrWhiteSpace(columnName))
        {
            if (columnName.Length > 100)
                errors.Add("Column name cannot exceed 100 characters.");

            if (!Regex.IsMatch(columnName, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
                errors.Add("Column name must start with a letter and contain only letters, numbers, and underscores.");

            // Check that it has a valid publisher prefix
            if (!Regex.IsMatch(columnName, @"^(new_|cr[a-f0-9]{2,5}_|[a-z]{2,8}_)[a-z][a-z0-9_]*$"))
                errors.Add("Column name must include a valid publisher prefix (e.g., 'new_', 'cr123_', or custom publisher prefix).");

            // Check for reserved words (after prefix)
            var nameWithoutPrefix = columnName.Contains('_') ? columnName.Substring(columnName.IndexOf('_') + 1) : columnName;
            var reservedWords = new[] { "id", "createdby", "createdon", "modifiedby", "modifiedon", "ownerid", "owningbusinessunit", "statecode", "statuscode" };
            if (reservedWords.Contains(nameWithoutPrefix.ToLower()))
                errors.Add($"Column name '{nameWithoutPrefix}' (after prefix) is a reserved word and cannot be used.");
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    /// <summary>
    /// Validates and cleans the column name (ensures proper format but doesn't add prefixes)
    /// </summary>
    private static string ValidateAndCleanColumnName(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return columnName;

        // Convert to lowercase and clean up
        var result = columnName.ToLower();

        // Replace spaces and special characters with underscores (but preserve the prefix structure)
        var parts = result.Split('_', 2);
        if (parts.Length == 2)
        {
            // Keep the prefix as-is, clean the rest
            var prefix = parts[0];
            var name = parts[1];
            name = Regex.Replace(name, @"[^a-z0-9_]", "_");
            name = Regex.Replace(name, @"_{2,}", "_");
            name = name.Trim('_');
            result = prefix + "_" + name;
        }
        else
        {
            // No underscore found, clean the whole thing
            result = Regex.Replace(result, @"[^a-z0-9_]", "_");
            result = Regex.Replace(result, @"_{2,}", "_");
            result = result.Trim('_');
        }

        return result;
    }    /// <summary>
         /// Converts logical name to schema name format (now returns same as logical name)
         /// </summary>
    private static string ConvertToSchemaName(string logicalName)
    {
        // Schema name should be the same as logical name (lowercase)
        return logicalName;
    }

    /// <summary>
    /// Creates a success response JSON
    /// </summary>
    private static string CreateSuccessResponse(string message, object data)
    {
        var response = new
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    }    /// <summary>
         /// Creates an error response JSON
         /// </summary>
    private static string CreateErrorResponse(string message, IEnumerable<string> errors)
    {
        var response = new
        {
            Success = false,
            Message = message,
            Errors = errors.ToArray(),
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    }

    #endregion

    #region Column Deletion Methods

    /// <summary>
    /// Deletes a column from a Dataverse table, providing comprehensive error information about dependencies
    /// </summary>
    /// <param name="tableName">The logical name of the table containing the column</param>
    /// <param name="columnName">The logical name of the column to delete</param>
    /// <returns>JSON string containing the result of the operation with detailed error information</returns>
    [McpServerTool, Description("Deletes a column from a Dataverse table with comprehensive dependency analysis and error reporting.")]
    public static async Task<string> DeleteColumn(string tableName, string columnName)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(tableName))
                return CreateErrorResponse("Input Validation Failed", new[] { "Table name is required." });

            if (string.IsNullOrWhiteSpace(columnName))
                return CreateErrorResponse("Input Validation Failed", new[] { "Column name is required." });

            var serviceClient = ConfigurationHelper.GetServiceClient();

            // First, check if the column exists and get its metadata
            var retrieveRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = tableName,
                LogicalName = columnName.ToLower()
            };

            var retrieveResponse = (RetrieveAttributeResponse)await serviceClient.ExecuteAsync(retrieveRequest);
            var attributeMetadata = retrieveResponse.AttributeMetadata;

            // Perform the delete operation
            var deleteRequest = new DeleteAttributeRequest
            {
                EntityLogicalName = tableName,
                LogicalName = columnName.ToLower()
            };

            await serviceClient.ExecuteAsync(deleteRequest);

            return CreateSuccessResponse("Column Deleted Successfully", new
            {
                TableName = tableName,
                ColumnName = columnName.ToLower(),
                AttributeType = attributeMetadata.AttributeType?.ToString(),
                DisplayName = attributeMetadata.DisplayName?.UserLocalizedLabel?.Label
            });
        }
        catch (Exception ex)
        {
            // Parse the error to provide comprehensive dependency information
            return await HandleColumnDeletionError(tableName, columnName, ex, ConfigurationHelper.GetServiceClient());
        }
    }

    /// <summary>
    /// Handles column deletion errors and provides comprehensive dependency analysis
    /// </summary>
    private static async Task<string> HandleColumnDeletionError(string tableName, string columnName, Exception originalException, ServiceClient serviceClient)
    {
        var errorDetails = new List<string> { $"Failed to delete column '{columnName}' from table '{tableName}': {originalException.Message}" };

        try
        {
            // Check if column exists
            var columnExists = await CheckColumnExists(tableName, columnName, serviceClient);
            if (!columnExists)
            {
                errorDetails.Add($"Column '{columnName}' does not exist in table '{tableName}'.");
            }

            // If it's a dependency error, provide troubleshooting guidance
            if (originalException.Message.Contains("dependencies") || originalException.Message.Contains("reference"))
            {
                errorDetails.Add("This column has dependencies that prevent deletion. Common dependencies include:");
                errorDetails.Add("- Forms or views that display this column");
                errorDetails.Add("- Business rules or workflows that use this column");
                errorDetails.Add("- Calculated or rollup fields that reference this column");
                errorDetails.Add("- Custom code or plugins that reference this column");
                errorDetails.Add("- Active relationships (for lookup columns)");
            }
        }
        catch (Exception analysisEx)
        {
            errorDetails.Add($"Error during dependency analysis: {analysisEx.Message}");
        }

        var errorResponse = new
        {
            Success = false,
            Message = "Failed to Delete Column",
            Errors = errorDetails.ToArray(),
            TableName = tableName,
            ColumnName = columnName,
            Timestamp = DateTime.UtcNow,
            TroubleshootingTips = new[]
            {
                "Remove the column from all forms and views before deletion",
                "Check and remove any business rules that reference this column",
                "Verify no workflows or Power Automate flows use this column",
                "Ensure no calculated fields or rollup fields reference this column",
                "For lookup columns, check for active relationships",
                "Verify you have sufficient permissions to delete the column"
            }
        };

        return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
    }    /// <summary>
         /// Checks if a column exists in the specified table
         /// </summary>
    private static async Task<bool> CheckColumnExists(string tableName, string columnName, ServiceClient serviceClient)
    {
        try
        {
            var retrieveRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = tableName,
                LogicalName = columnName.ToLower()
            };

            await serviceClient.ExecuteAsync(retrieveRequest);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the primary key attribute name for a given table
    /// </summary>
    private static async Task<string?> GetPrimaryKeyAttributeName(ServiceClient serviceClient, string tableName)
    {
        try
        {
            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Attributes
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var primaryKey = response.EntityMetadata.Attributes
                .FirstOrDefault(attr => attr.IsPrimaryId == true);

            return primaryKey?.LogicalName;
        }
        catch (Exception)
        {
            // Fallback: for standard tables, the primary key usually follows the pattern {tablename}id
            return $"{tableName}id";
        }
    }

    #endregion
}
