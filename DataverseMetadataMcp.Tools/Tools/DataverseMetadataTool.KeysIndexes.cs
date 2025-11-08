using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using ModelContextProtocol.Server;
using DataverseMetadataMcp.Tools.Configuration;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace DataverseMetadataMcp.Tools.Tools;

/// <summary>
/// MCP tools for retrieving Dataverse metadata information
/// </summary>
public static partial class DataverseMetadataTool
{
    #region Keys and Indexes Metadata Methods

    /// <summary>
    /// Retrieves all alternate keys for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing alternate keys information</returns>
    [McpServerTool, Description("Retrieves all alternate keys for a specific table from Dataverse.")]
    public static async Task<string> ReadEntityKeys(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var keys = entity.Keys?.Select(k => new
            {
                LogicalName = k.LogicalName,
                SchemaName = k.SchemaName,
                DisplayName = k.DisplayName?.UserLocalizedLabel?.Label ?? k.LogicalName,
                KeyAttributes = k.KeyAttributes?.ToList(),
                IsCustomizable = k.IsCustomizable?.Value,
                IsManaged = k.IsManaged,
                IntroducedVersion = k.IntroducedVersion,
                EntityKeyIndexStatus = k.EntityKeyIndexStatus.ToString()
            }).ToList();

            return JsonSerializer.Serialize(keys, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving entity keys: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves primary key information for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing primary key information</returns>
    [McpServerTool, Description("Retrieves primary key information for a specific table from Dataverse.")]
    public static async Task<string> ReadPrimaryKey(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Entity | EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var primaryKeyAttribute = entity.Attributes?.FirstOrDefault(a => a.IsPrimaryId == true);

            var primaryKey = new
            {
                TableLogicalName = entity.LogicalName,
                TableDisplayName = entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName,
                PrimaryIdAttribute = entity.PrimaryIdAttribute,
                PrimaryNameAttribute = entity.PrimaryNameAttribute,
                PrimaryKeyDetails = primaryKeyAttribute != null ? new
                {
                    LogicalName = primaryKeyAttribute.LogicalName,
                    SchemaName = primaryKeyAttribute.SchemaName,
                    DisplayName = primaryKeyAttribute.DisplayName?.UserLocalizedLabel?.Label ?? primaryKeyAttribute.LogicalName,
                    AttributeType = primaryKeyAttribute.AttributeType?.ToString(),
                    IsCustomAttribute = primaryKeyAttribute.IsCustomAttribute,
                    Description = primaryKeyAttribute.Description?.UserLocalizedLabel?.Label
                } : null
            };

            return JsonSerializer.Serialize(primaryKey, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving primary key: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all indexes for a specific table (via alternate keys)
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing index information</returns>
    [McpServerTool, Description("Retrieves all indexes for a specific table through alternate keys metadata.")]
    public static async Task<string> ReadTableIndexes(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveEntityRequest
            {
                LogicalName = tableName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);
            var entity = response.EntityMetadata;

            var indexes = new
            {
                TableLogicalName = entity.LogicalName,
                TableDisplayName = entity.DisplayName?.UserLocalizedLabel?.Label ?? entity.LogicalName,
                PrimaryIndex = new
                {
                    Type = "Primary Key",
                    AttributeName = entity.PrimaryIdAttribute,
                    IsUnique = true,
                    IsClustered = true
                },
                AlternateKeyIndexes = entity.Keys?.Select(k => new
                {
                    Type = "Alternate Key",
                    KeyName = k.LogicalName,
                    KeyDisplayName = k.DisplayName?.UserLocalizedLabel?.Label ?? k.LogicalName,
                    Attributes = k.KeyAttributes?.ToList(),
                    IsUnique = true,
                    IsClustered = false,
                    IndexStatus = k.EntityKeyIndexStatus.ToString(),
                    IsCustomizable = k.IsCustomizable?.Value,
                    IsManaged = k.IsManaged
                }).ToList(),
                IndexCount = 1 + (entity.Keys?.Length ?? 0) // Primary + Alternate keys
            };

            return JsonSerializer.Serialize(indexes, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving table indexes: {ex.Message}";
        }
    }

    #endregion
}
