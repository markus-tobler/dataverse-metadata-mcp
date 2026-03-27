using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using ModelContextProtocol.Server;
using DataverseMetadataMcp.Tools.Configuration;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;

namespace DataverseMetadataMcp.Tools.Tools;

/// <summary>
/// MCP tools for retrieving Dataverse metadata information
/// </summary>
[McpServerToolType]
public static partial class DataverseMetadataTool
{
    #region Table and Column Metadata Methods

    /// <summary>
    /// Retrieves all tables (entities) from Dataverse
    /// </summary>
    /// <returns>JSON string containing list of tables with their metadata</returns>
    [McpServerTool, Description("Retrieves all tables (entities) from Dataverse using the configured connection string.")]
    public static async Task<string> ReadTables()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveAllEntitiesRequest
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveAllEntitiesResponse)await serviceClient.ExecuteAsync(request);

            var tables = response.EntityMetadata
                .Where(e => e.IsCustomizable?.Value == true || e.IsManaged == false)
                .Select(e => new
                {
                    LogicalName = e.LogicalName,
                    DisplayName = e.DisplayName?.UserLocalizedLabel?.Label ?? e.LogicalName,
                    SchemaName = e.SchemaName,
                    IsCustomEntity = e.IsCustomEntity,
                    Description = e.Description?.UserLocalizedLabel?.Label
                })
                .OrderBy(e => e.DisplayName)
                .ToList();

            return JsonSerializer.Serialize(tables, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving tables: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing detailed table information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific table (entity) from Dataverse using the table's logical name.")]
    public static async Task<string> ReadTable(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var tableDetails = new
            {
                LogicalName = entity.LogicalName,
                DisplayName = entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName,
                SchemaName = entity.SchemaName,
                PrimaryNameAttribute = entity.PrimaryNameAttribute,
                PrimaryIdAttribute = entity.PrimaryIdAttribute,
                Description = entity.Description?.UserLocalizedLabel?.Label,
                IsCustomEntity = entity.IsCustomEntity,
                IsActivity = entity.IsActivity,
                IsBusinessProcessEnabled = entity.IsBusinessProcessEnabled,
                IsAuditEnabled = entity.IsAuditEnabled?.Value,
                AttributeCount = entity.Attributes?.Length ?? 0,
                RelationshipCount = (entity.OneToManyRelationships?.Length ?? 0) + (entity.ManyToOneRelationships?.Length ?? 0) + (entity.ManyToManyRelationships?.Length ?? 0)
            };

            return JsonSerializer.Serialize(tableDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving table details: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all columns (attributes) for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing list of columns with their metadata</returns>
    [McpServerTool, Description("Retrieves all columns (attributes) for a specific table from Dataverse using the table's logical name.")]
    public static async Task<string> ReadColumns(string tableName)
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

            var columns = entity.Attributes
                .Select(a => new
                {
                    LogicalName = a.LogicalName,
                    DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                    SchemaName = a.SchemaName,
                    AttributeType = a.AttributeType?.ToString(),
                    DataType = GetDataTypeDescription(a),
                    IsCustomAttribute = a.IsCustomAttribute,
                    IsPrimaryId = a.IsPrimaryId,
                    IsPrimaryName = a.IsPrimaryName,
                    IsValidForCreate = a.IsValidForCreate,
                    IsValidForUpdate = a.IsValidForUpdate,
                    IsValidForRead = a.IsValidForRead,
                    RequiredLevel = a.RequiredLevel?.ToString(),
                    MaxLength = GetMaxLength(a),
                    Description = a.Description?.UserLocalizedLabel?.Label
                })
                .OrderBy(a => a.DisplayName)
                .ToList();

            return JsonSerializer.Serialize(columns, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving columns: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a human-readable description of the attribute data type
    /// </summary>
    /// <param name="attribute">The attribute metadata</param>
    /// <returns>A descriptive string of the data type</returns>
    private static string GetDataTypeDescription(AttributeMetadata attribute)
    {
        return attribute switch
        {
            StringAttributeMetadata stringAttr => $"String (Max: {stringAttr.MaxLength})",
            IntegerAttributeMetadata intAttr => $"Integer (Min: {intAttr.MinValue}, Max: {intAttr.MaxValue})",
            DecimalAttributeMetadata decAttr => $"Decimal (Precision: {decAttr.Precision})",
            DoubleAttributeMetadata doubleAttr => $"Double (Min: {doubleAttr.MinValue}, Max: {doubleAttr.MaxValue})",
            MoneyAttributeMetadata moneyAttr => $"Money (Min: {moneyAttr.MinValue}, Max: {moneyAttr.MaxValue})",
            DateTimeAttributeMetadata dateAttr => $"DateTime (Format: {dateAttr.Format})",
            BooleanAttributeMetadata => "Boolean",
            LookupAttributeMetadata lookupAttr => $"Lookup (Targets: {string.Join(", ", lookupAttr.Targets ?? Array.Empty<string>())})",
            PicklistAttributeMetadata => "Choice (Picklist)",
            MultiSelectPicklistAttributeMetadata => "Choices (Multi-select)",
            StatusAttributeMetadata => "Status",
            StateAttributeMetadata => "State",
            MemoAttributeMetadata memoAttr => $"Multiline Text (Max: {memoAttr.MaxLength})",
            ImageAttributeMetadata => "Image",
            FileAttributeMetadata => "File",
            _ => attribute.AttributeType?.ToString() ?? "Unknown"
        };
    }

    /// <summary>
    /// Gets the maximum length for text-based attributes
    /// </summary>
    /// <param name="attribute">The attribute metadata</param>
    /// <returns>The maximum length if applicable, null otherwise</returns>
    private static int? GetMaxLength(AttributeMetadata attribute)
    {
        return attribute switch
        {
            StringAttributeMetadata stringAttr => stringAttr.MaxLength,
            MemoAttributeMetadata memoAttr => memoAttr.MaxLength,
            _ => null
        };
    }

    #endregion

    #region Table Creation Methods

    /// <summary>
    /// Creates a new table (entity) in Dataverse
    /// </summary>
    /// <param name="schemaName">The schema name for the table (e.g., new_MyTable) - will be converted to lowercase</param>
    /// <param name="displayName">The display name for the table</param>
    /// <param name="pluralName">The plural display name for the table</param>
    /// <param name="description">Optional description for the table</param>
    /// <param name="primaryNameDisplayName">Display name for the primary name column (default: Name)</param>
    /// <param name="primaryNameSchemaName">Schema name for the primary name column (default: new_name) - will be converted to lowercase</param>
    /// <param name="primaryNameMaxLength">Maximum length for the primary name column (default: 100, max: 4000)</param>
    /// <param name="languageCode">Language code for labels (default: 1033 for English)</param>
    /// <param name="enableAudit">Enable audit for the table (default: false)</param>
    /// <param name="enableNotes">Enable notes (attachments) for the table (default: false)</param>
    /// <param name="enableActivities">Enable activities for the table (default: false)</param>
    /// <param name="enableConnections">Enable connections for the table (default: false)</param>
    /// <param name="enableMailMerge">Enable mail merge for the table (default: false)</param>
    /// <param name="enableQueues">Enable queues for the table (default: false)</param>
    /// <param name="enableBusinessProcessFlow">Enable business process flows for the table (default: false)</param>
    /// <param name="ownershipType">Ownership type for the table: 'UserOwned' (user/team owned, default) or 'OrganizationOwned'.</param>
    /// <param name="solutionUniqueName">(Optional, lowercase) If provided, adds the new table to the specified unmanaged solution using its unique name.</param>
    /// <returns>JSON string containing the result of the table creation operation</returns>
    [McpServerTool, Description("Creates a new table (entity) in Dataverse with specified configuration options.")]
    public static async Task<string> CreateTable(
    string schemaName,
    string displayName,
    string pluralName,
    string? description = null,
    string primaryNameDisplayName = "Name",
    string? primaryNameSchemaName = null,
    int primaryNameMaxLength = 100,
    int languageCode = 1033,
    bool enableAudit = false,
    bool enableNotes = false,
    bool enableActivities = false,
    bool enableConnections = false,
    bool enableMailMerge = false,
    bool enableQueues = false,
    bool enableBusinessProcessFlow = false,
    string ownershipType = "UserOwned",
    string? solutionUniqueName = null)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Validation
            if (string.IsNullOrWhiteSpace(schemaName))
                return "Error: Schema name is required.";

            if (string.IsNullOrWhiteSpace(displayName))
                return "Error: Display name is required.";

            if (string.IsNullOrWhiteSpace(pluralName))
                return "Error: Plural name is required.";

            // Convert schema name to lowercase
            schemaName = schemaName.ToLowerInvariant();

            // Validate schema name format
            if (!schemaName.Contains("_"))
                return "Error: Schema name must contain an underscore (e.g., new_mytable).";

            // Validate primary name column length
            if (primaryNameMaxLength < 1 || primaryNameMaxLength > 4000)
                return "Error: Primary name max length must be between 1 and 4000.";

            // Validate and map ownership type
            OwnershipTypes resolvedOwnershipType;
            if (string.Equals(ownershipType, "OrganizationOwned", StringComparison.OrdinalIgnoreCase))
                resolvedOwnershipType = OwnershipTypes.OrganizationOwned;
            else if (string.Equals(ownershipType, "UserOwned", StringComparison.OrdinalIgnoreCase))
                resolvedOwnershipType = OwnershipTypes.UserOwned;
            else
                return "Error: ownershipType must be 'UserOwned' or 'OrganizationOwned'.";

            // Set default primary name schema name if not provided
            if (string.IsNullOrWhiteSpace(primaryNameSchemaName))
            {
                var prefix = schemaName.Split('_')[0];
                primaryNameSchemaName = $"{prefix}_name";
            }
            else
            {
                // Convert primary name schema name to lowercase as well
                primaryNameSchemaName = primaryNameSchemaName.ToLowerInvariant();
            }

            // Create the entity metadata
            var entityMetadata = new EntityMetadata
            {
                SchemaName = schemaName,
                DisplayName = new Label(displayName, languageCode),
                DisplayCollectionName = new Label(pluralName, languageCode),
                Description = !string.IsNullOrWhiteSpace(description) ? new Label(description, languageCode) : null,
                OwnershipType = resolvedOwnershipType,
                IsActivity = false,
                IsAvailableOffline = true,
                IsAuditEnabled = new BooleanManagedProperty(enableAudit),
                HasNotes = enableNotes,
                HasActivities = enableActivities,
                IsConnectionsEnabled = new BooleanManagedProperty(enableConnections),
                IsMailMergeEnabled = new BooleanManagedProperty(enableMailMerge),
                IsValidForQueue = new BooleanManagedProperty(enableQueues),
                IsBusinessProcessEnabled = enableBusinessProcessFlow
            };

            // Create the primary name attribute
            var primaryNameAttribute = new StringAttributeMetadata
            {
                SchemaName = primaryNameSchemaName,
                DisplayName = new Label(primaryNameDisplayName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None),
                MaxLength = primaryNameMaxLength,
                Format = StringFormat.Text
            };

            // Create the table
            var createRequest = new CreateEntityRequest
            {
                Entity = entityMetadata,
                PrimaryAttribute = primaryNameAttribute,
                HasNotes = enableNotes,
                HasActivities = enableActivities
            };
            // If solutionUniqueName is provided, add the table to the solution
            if (!string.IsNullOrWhiteSpace(solutionUniqueName))
            {
                createRequest.SolutionUniqueName = solutionUniqueName.ToLowerInvariant();
            }

            var createResponse = (CreateEntityResponse)await serviceClient.ExecuteAsync(createRequest);

            // Publish the customizations for the new entity
            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $@"<importexportxml>
                                    <entities>
                                        <entity>{schemaName}</entity>
                                    </entities>
                                  </importexportxml>"
            };
            await serviceClient.ExecuteAsync(publishRequest);

            var result = new
            {
                Success = true,
                Message = $"Table '{displayName}' created successfully.",
                EntityId = createResponse.EntityId,
                TableDetails = new
                {
                    SchemaName = schemaName,
                    DisplayName = displayName,
                    PluralName = pluralName,
                    Description = description,
                    LanguageCode = languageCode,
                    PrimaryNameColumn = new
                    {
                        SchemaName = primaryNameSchemaName,
                        DisplayName = primaryNameDisplayName,
                        MaxLength = primaryNameMaxLength
                    },
                    Features = new
                    {
                        AuditEnabled = enableAudit,
                        NotesEnabled = enableNotes,
                        ActivitiesEnabled = enableActivities,
                        ConnectionsEnabled = enableConnections,
                        MailMergeEnabled = enableMailMerge,
                        QueuesEnabled = enableQueues,
                        BusinessProcessFlowEnabled = enableBusinessProcessFlow
                    }
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            var errorResult = new
            {
                Success = false,
                Message = $"Error creating table: {ex.Message}",
                ErrorDetails = ex.InnerException?.Message
            };

            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    #endregion

    #region Relationship Metadata Methods

    /// <summary>
    /// Retrieves all relationships for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing all relationships (1:N, N:1, N:N) for the table</returns>
    [McpServerTool, Description("Retrieves all relationships (1:N, N:1, N:N) for a specific table from Dataverse.")]
    public static async Task<string> ReadEntityRelationships(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var relationships = new
            {
                OneToManyRelationships = entity.OneToManyRelationships?.Select(r => new
                {
                    SchemaName = r.SchemaName,
                    ReferencedEntity = r.ReferencedEntity,
                    ReferencingEntity = r.ReferencingEntity,
                    ReferencedAttribute = r.ReferencedAttribute,
                    ReferencingAttribute = r.ReferencingAttribute,
                    RelationshipType = "OneToMany",
                    IsHierarchical = r.IsHierarchical,
                    IsCustomRelationship = r.IsCustomRelationship,
                    CascadeConfiguration = new
                    {
                        Assign = r.CascadeConfiguration?.Assign.ToString(),
                        Delete = r.CascadeConfiguration?.Delete.ToString(),
                        Merge = r.CascadeConfiguration?.Merge.ToString(),
                        Reparent = r.CascadeConfiguration?.Reparent.ToString(),
                        Share = r.CascadeConfiguration?.Share.ToString(),
                        Unshare = r.CascadeConfiguration?.Unshare.ToString()
                    }
                }).ToList(),

                ManyToOneRelationships = entity.ManyToOneRelationships?.Select(r => new
                {
                    SchemaName = r.SchemaName,
                    ReferencedEntity = r.ReferencedEntity,
                    ReferencingEntity = r.ReferencingEntity,
                    ReferencedAttribute = r.ReferencedAttribute,
                    ReferencingAttribute = r.ReferencingAttribute,
                    RelationshipType = "ManyToOne",
                    IsCustomRelationship = r.IsCustomRelationship
                }).ToList(),

                ManyToManyRelationships = entity.ManyToManyRelationships?.Select(r => new
                {
                    SchemaName = r.SchemaName,
                    Entity1LogicalName = r.Entity1LogicalName,
                    Entity2LogicalName = r.Entity2LogicalName,
                    Entity1IntersectAttribute = r.Entity1IntersectAttribute,
                    Entity2IntersectAttribute = r.Entity2IntersectAttribute,
                    IntersectEntityName = r.IntersectEntityName,
                    RelationshipType = "ManyToMany",
                    IsCustomRelationship = r.IsCustomRelationship
                }).ToList()
            };

            return JsonSerializer.Serialize(relationships, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving entity relationships: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves one-to-many relationships for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing one-to-many relationships</returns>
    [McpServerTool, Description("Retrieves one-to-many relationships for a specific table from Dataverse.")]
    public static async Task<string> ReadOneToManyRelationships(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var oneToManyRelationships = entity.OneToManyRelationships?.Select(r => new
            {
                SchemaName = r.SchemaName,
                ReferencedEntity = r.ReferencedEntity,
                ReferencingEntity = r.ReferencingEntity,
                ReferencedAttribute = r.ReferencedAttribute,
                ReferencingAttribute = r.ReferencingAttribute,
                IsHierarchical = r.IsHierarchical,
                IsCustomRelationship = r.IsCustomRelationship,
                RelationshipBehavior = r.RelationshipBehavior?.ToString(),
                SecurityTypes = r.SecurityTypes?.ToString(),
                CascadeConfiguration = new
                {
                    Assign = r.CascadeConfiguration?.Assign.ToString(),
                    Delete = r.CascadeConfiguration?.Delete.ToString(),
                    Merge = r.CascadeConfiguration?.Merge.ToString(),
                    Reparent = r.CascadeConfiguration?.Reparent.ToString(),
                    Share = r.CascadeConfiguration?.Share.ToString(),
                    Unshare = r.CascadeConfiguration?.Unshare.ToString()
                }
            }).ToList();

            return JsonSerializer.Serialize(oneToManyRelationships, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving one-to-many relationships: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves many-to-one relationships for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing many-to-one relationships</returns>
    [McpServerTool, Description("Retrieves many-to-one relationships for a specific table from Dataverse.")]
    public static async Task<string> ReadManyToOneRelationships(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var manyToOneRelationships = entity.ManyToOneRelationships?.Select(r => new
            {
                SchemaName = r.SchemaName,
                ReferencedEntity = r.ReferencedEntity,
                ReferencingEntity = r.ReferencingEntity,
                ReferencedAttribute = r.ReferencedAttribute,
                ReferencingAttribute = r.ReferencingAttribute,
                IsCustomRelationship = r.IsCustomRelationship,
                RelationshipBehavior = r.RelationshipBehavior?.ToString(),
                SecurityTypes = r.SecurityTypes?.ToString()
            }).ToList();

            return JsonSerializer.Serialize(manyToOneRelationships, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving many-to-one relationships: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves many-to-many relationships for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing many-to-many relationships</returns>
    [McpServerTool, Description("Retrieves many-to-many relationships for a specific table from Dataverse.")]
    public static async Task<string> ReadManyToManyRelationships(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Relationships,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var manyToManyRelationships = entity.ManyToManyRelationships?.Select(r => new
            {
                SchemaName = r.SchemaName,
                Entity1LogicalName = r.Entity1LogicalName,
                Entity2LogicalName = r.Entity2LogicalName,
                Entity1IntersectAttribute = r.Entity1IntersectAttribute,
                Entity2IntersectAttribute = r.Entity2IntersectAttribute,
                IntersectEntityName = r.IntersectEntityName,
                IsCustomRelationship = r.IsCustomRelationship,
                Entity1NavigationPropertyName = r.Entity1NavigationPropertyName,
                Entity2NavigationPropertyName = r.Entity2NavigationPropertyName,
                IsValidForAdvancedFind = r.IsValidForAdvancedFind
            }).ToList();

            return JsonSerializer.Serialize(manyToManyRelationships, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving many-to-many relationships: {ex.Message}";
        }
    }

    #endregion
}
