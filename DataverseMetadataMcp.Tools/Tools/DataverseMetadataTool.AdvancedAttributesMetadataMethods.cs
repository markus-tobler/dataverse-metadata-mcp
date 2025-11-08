using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using ModelContextProtocol.Server;
using DataverseMetadataMcp.Tools.Configuration;
using System.ComponentModel;
using System.Text.Json;

namespace DataverseMetadataMcp.Tools.Tools;

/// <summary>
/// MCP tools for retrieving Dataverse metadata information
/// </summary>
public static partial class DataverseMetadataTool
{
    #region Advanced Attributes Metadata Methods

    /// <summary>
    /// Retrieves detailed information about a specific attribute
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="attributeName">The logical name of the attribute</param>
    /// <returns>JSON string containing detailed attribute information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific attribute from Dataverse.")]
    public static async Task<string> ReadAttributeDetails(string tableName, string attributeName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = tableName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAttributeResponse)await serviceClient.ExecuteAsync(request);
            var attribute = response.AttributeMetadata;

            var attributeDetails = new
            {
                LogicalName = attribute.LogicalName,
                DisplayName = attribute.DisplayName?.UserLocalizedLabel?.Label ?? attribute.LogicalName,
                SchemaName = attribute.SchemaName,
                AttributeType = attribute.AttributeType?.ToString(),
                AttributeTypeName = attribute.AttributeTypeName?.Value,
                Description = attribute.Description?.UserLocalizedLabel?.Label,
                IsCustomAttribute = attribute.IsCustomAttribute,
                IsPrimaryId = attribute.IsPrimaryId,
                IsPrimaryName = attribute.IsPrimaryName,
                IsValidForCreate = attribute.IsValidForCreate,
                IsValidForUpdate = attribute.IsValidForUpdate,
                IsValidForRead = attribute.IsValidForRead,
                IsValidForAdvancedFind = attribute.IsValidForAdvancedFind,
                RequiredLevel = attribute.RequiredLevel?.ToString(),
                CanBeSecuredForCreate = attribute.CanBeSecuredForCreate,
                CanBeSecuredForRead = attribute.CanBeSecuredForRead,
                CanBeSecuredForUpdate = attribute.CanBeSecuredForUpdate,
                IsAuditEnabled = attribute.IsAuditEnabled?.Value,
                IsManaged = attribute.IsManaged,
                IntroducedVersion = attribute.IntroducedVersion,
                IsRenameable = attribute.IsRenameable?.Value,
                IsValidODataAttribute = attribute.IsValidODataAttribute,
                IsFilterable = attribute.IsFilterable,
                IsSearchable = attribute.IsSearchable,
                IsRetrievable = attribute.IsRetrievable,
                IsSortableEnabled = attribute.IsSortableEnabled?.Value,
                DeprecatedVersion = attribute.DeprecatedVersion,
                ExternalName = attribute.ExternalName,
                DataTypeSpecific = GetAttributeTypeSpecificInfo(attribute)
            };

            return JsonSerializer.Serialize(attributeDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving attribute details: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves calculated fields for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing calculated fields for the table</returns>
    [McpServerTool, Description("Retrieves calculated fields for a specific table from Dataverse.")]
    public static async Task<string> ReadCalculatedFields(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var calculatedFields = entity.Attributes
                .Where(a => a.SourceType == 1) // Calculated
                .Select(a => new
                {
                    LogicalName = a.LogicalName,
                    DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                    SchemaName = a.SchemaName,
                    AttributeType = a.AttributeType?.ToString(),
                    DataType = GetDataTypeDescription(a),
                    SourceType = a.SourceType,
                    SourceTypeName = GetSourceTypeName(a.SourceType ?? 0),
                    IsCustomAttribute = a.IsCustomAttribute,
                    RequiredLevel = a.RequiredLevel?.ToString(),
                    Description = a.Description?.UserLocalizedLabel?.Label,
                    FormulaDefinition = GetFormulaDefinition(a),
                    IsValidForCreate = a.IsValidForCreate,
                    IsValidForUpdate = a.IsValidForUpdate,
                    IsValidForRead = a.IsValidForRead
                })
                .OrderBy(a => a.DisplayName)
                .ToList();

            return JsonSerializer.Serialize(calculatedFields, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving calculated fields: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves rollup fields for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing rollup fields for the table</returns>
    [McpServerTool, Description("Retrieves rollup fields for a specific table from Dataverse.")]
    public static async Task<string> ReadRollupFields(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var rollupFields = entity.Attributes
                .Where(a => a.SourceType == 2) // Rollup
                .Select(a => new
                {
                    LogicalName = a.LogicalName,
                    DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                    SchemaName = a.SchemaName,
                    AttributeType = a.AttributeType?.ToString(),
                    DataType = GetDataTypeDescription(a),
                    SourceType = a.SourceType,
                    SourceTypeName = GetSourceTypeName(a.SourceType ?? 0),
                    IsCustomAttribute = a.IsCustomAttribute,
                    RequiredLevel = a.RequiredLevel?.ToString(),
                    Description = a.Description?.UserLocalizedLabel?.Label,
                    RollupState = GetRollupState(a),
                    IsValidForCreate = a.IsValidForCreate,
                    IsValidForUpdate = a.IsValidForUpdate,
                    IsValidForRead = a.IsValidForRead
                })
                .OrderBy(a => a.DisplayName)
                .ToList();

            return JsonSerializer.Serialize(rollupFields, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving rollup fields: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves lookup target information for a specific lookup attribute
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="attributeName">The logical name of the lookup attribute</param>
    /// <returns>JSON string containing lookup target information</returns>
    [McpServerTool, Description("Retrieves lookup target information for a specific lookup attribute from Dataverse.")]
    public static async Task<string> ReadLookupTargets(string tableName, string attributeName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = tableName,
                LogicalName = attributeName,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAttributeResponse)await serviceClient.ExecuteAsync(request);
            var attribute = response.AttributeMetadata;

            if (attribute is not LookupAttributeMetadata lookupAttr)
            {
                return JsonSerializer.Serialize(new { Error = "Attribute is not a lookup type" }, new JsonSerializerOptions { WriteIndented = true });
            }
            var lookupTargets = new
            {
                AttributeName = lookupAttr.LogicalName,
                DisplayName = lookupAttr.DisplayName?.UserLocalizedLabel?.Label ?? lookupAttr.LogicalName,
                AttributeType = "Lookup",
                Targets = lookupAttr.Targets,
                TargetCount = lookupAttr.Targets?.Length ?? 0,
                IsPolymorphic = (lookupAttr.Targets?.Length ?? 0) > 1,
                Format = lookupAttr.Format?.ToString(),
                IsValidForCreate = lookupAttr.IsValidForCreate,
                IsValidForUpdate = lookupAttr.IsValidForUpdate,
                IsValidForRead = lookupAttr.IsValidForRead,
                RequiredLevel = lookupAttr.RequiredLevel?.ToString(),
                Description = lookupAttr.Description?.UserLocalizedLabel?.Label
            };

            return JsonSerializer.Serialize(lookupTargets, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving lookup targets: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets type-specific information for an attribute
    /// </summary>
    /// <param name="attribute">The attribute metadata</param>
    /// <returns>An object with type-specific information</returns>
    private static object GetAttributeTypeSpecificInfo(AttributeMetadata attribute)
    {
        return attribute switch
        {
            StringAttributeMetadata stringAttr => new
            {
                MaxLength = stringAttr.MaxLength,
                Format = stringAttr.Format?.ToString(),
                FormatName = stringAttr.FormatName?.Value,
                ImeMode = stringAttr.ImeMode?.ToString(),
                DatabaseLength = stringAttr.DatabaseLength,
                IsLocalizable = stringAttr.IsLocalizable
            },
            IntegerAttributeMetadata intAttr => new
            {
                MinValue = intAttr.MinValue,
                MaxValue = intAttr.MaxValue,
                Format = intAttr.Format?.ToString()
            },
            DecimalAttributeMetadata decAttr => new
            {
                MinValue = decAttr.MinValue,
                MaxValue = decAttr.MaxValue,
                Precision = decAttr.Precision,
                ImeMode = decAttr.ImeMode?.ToString()
            },
            DoubleAttributeMetadata doubleAttr => new
            {
                MinValue = doubleAttr.MinValue,
                MaxValue = doubleAttr.MaxValue,
                Precision = doubleAttr.Precision,
                ImeMode = doubleAttr.ImeMode?.ToString()
            },
            MoneyAttributeMetadata moneyAttr => new
            {
                MinValue = moneyAttr.MinValue,
                MaxValue = moneyAttr.MaxValue,
                Precision = moneyAttr.Precision,
                PrecisionSource = moneyAttr.PrecisionSource,
                ImeMode = moneyAttr.ImeMode?.ToString(),
                CalculationOf = moneyAttr.CalculationOf,
                FormulaDefinition = moneyAttr.FormulaDefinition,
                SourceTypeMask = moneyAttr.SourceTypeMask
            },
            DateTimeAttributeMetadata dateAttr => new
            {
                Format = dateAttr.Format?.ToString(),
                ImeMode = dateAttr.ImeMode?.ToString(),
                DateTimeBehavior = dateAttr.DateTimeBehavior?.Value,
                CanChangeDateTimeBehavior = dateAttr.CanChangeDateTimeBehavior?.Value
            },
            BooleanAttributeMetadata boolAttr => new
            {
                DefaultValue = boolAttr.DefaultValue,
                OptionSet = boolAttr.OptionSet != null ? new
                {
                    TrueOption = new
                    {
                        Value = boolAttr.OptionSet.TrueOption?.Value,
                        Label = boolAttr.OptionSet.TrueOption?.Label?.UserLocalizedLabel?.Label
                    },
                    FalseOption = new
                    {
                        Value = boolAttr.OptionSet.FalseOption?.Value,
                        Label = boolAttr.OptionSet.FalseOption?.Label?.UserLocalizedLabel?.Label
                    }
                } : null
            },
            LookupAttributeMetadata lookupAttr => new
            {
                Targets = lookupAttr.Targets,
                Format = lookupAttr.Format?.ToString()
            },
            PicklistAttributeMetadata picklistAttr => new
            {
                DefaultFormValue = picklistAttr.DefaultFormValue,
                OptionSetName = picklistAttr.OptionSet?.Name,
                IsGlobal = picklistAttr.OptionSet?.IsGlobal,
                OptionCount = picklistAttr.OptionSet?.Options?.Count
            },
            MultiSelectPicklistAttributeMetadata multiSelectAttr => new
            {
                DefaultFormValue = multiSelectAttr.DefaultFormValue,
                OptionSetName = multiSelectAttr.OptionSet?.Name,
                IsGlobal = multiSelectAttr.OptionSet?.IsGlobal,
                OptionCount = multiSelectAttr.OptionSet?.Options?.Count
            },
            MemoAttributeMetadata memoAttr => new
            {
                MaxLength = memoAttr.MaxLength,
                Format = memoAttr.Format?.ToString(),
                ImeMode = memoAttr.ImeMode?.ToString(),
                IsLocalizable = memoAttr.IsLocalizable
            },
            ImageAttributeMetadata imageAttr => new
            {
                MaxHeight = imageAttr.MaxHeight,
                MaxWidth = imageAttr.MaxWidth,
                CanStoreFullImage = imageAttr.CanStoreFullImage
            },
            FileAttributeMetadata fileAttr => new
            {
                MaxSizeInKB = fileAttr.MaxSizeInKB
            },
            _ => new { Type = attribute.GetType().Name }
        };
    }

    /// <summary>
    /// Gets a human-readable name for source type values
    /// </summary>
    /// <param name="sourceType">The source type value</param>
    /// <returns>A descriptive string of the source type</returns>
    private static string GetSourceTypeName(int sourceType)
    {
        return sourceType switch
        {
            0 => "Simple",
            1 => "Calculated",
            2 => "Rollup",
            _ => $"Unknown ({sourceType})"
        };
    }

    /// <summary>
    /// Gets formula definition for calculated fields
    /// </summary>
    /// <param name="attribute">The attribute metadata</param>
    /// <returns>Formula definition if available</returns>
    private static string? GetFormulaDefinition(AttributeMetadata attribute)
    {
        return attribute switch
        {
            StringAttributeMetadata stringAttr => stringAttr.FormulaDefinition,
            IntegerAttributeMetadata intAttr => intAttr.FormulaDefinition,
            DecimalAttributeMetadata decAttr => decAttr.FormulaDefinition,
            DoubleAttributeMetadata doubleAttr => doubleAttr.FormulaDefinition,
            MoneyAttributeMetadata moneyAttr => moneyAttr.FormulaDefinition,
            DateTimeAttributeMetadata dateAttr => dateAttr.FormulaDefinition,
            _ => null
        };
    }    /// <summary>
         /// Gets rollup state for rollup fields
         /// </summary>
         /// <param name="attribute">The attribute metadata</param>
         /// <returns>Rollup state information if available</returns>
    private static object? GetRollupState(AttributeMetadata attribute)
    {
        if (attribute.SourceType == 2) // Rollup
        {
            return new
            {
                IsRollupField = true,
                SourceType = attribute.SourceType,
                SourceTypeName = "Rollup"
            };
        }
        return null;
    }

    #endregion
}