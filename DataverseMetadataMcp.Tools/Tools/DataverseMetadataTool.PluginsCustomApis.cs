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
    #region Plugin and Custom API Metadata Methods

    /// <summary>
    /// Retrieves all plugin assemblies in the environment
    /// </summary>
    /// <returns>JSON string containing plugin assemblies information</returns>
    [McpServerTool, Description("Retrieves all plugin assemblies registered in the Dataverse environment.")]
    public static async Task<string> ReadPluginAssemblies()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var assemblyQuery = new QueryExpression("pluginassembly")
            {
                ColumnSet = new ColumnSet(
                    "pluginassemblyid", "name", "version", "culture", "publickeytoken",
                    "isolationmode", "sourcetype", "createdon", "modifiedon", "description"
                ),
                Orders = { new OrderExpression("name", OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(assemblyQuery);

            var assemblies = result.Entities.Select(e => new
            {
                PluginAssemblyId = e.GetAttributeValue<Guid>("pluginassemblyid"),
                Name = e.GetAttributeValue<string>("name"),
                Version = e.GetAttributeValue<string>("version"),
                Culture = e.GetAttributeValue<string>("culture"),
                PublicKeyToken = e.GetAttributeValue<string>("publickeytoken"),
                IsolationMode = GetIsolationModeText(e.GetAttributeValue<OptionSetValue>("isolationmode")?.Value),
                SourceType = GetSourceTypeText(e.GetAttributeValue<OptionSetValue>("sourcetype")?.Value),
                CreatedOn = e.GetAttributeValue<DateTime?>("createdon"),
                ModifiedOn = e.GetAttributeValue<DateTime?>("modifiedon"),
                Description = e.GetAttributeValue<string>("description")
            }).ToList();

            return JsonSerializer.Serialize(assemblies, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving plugin assemblies: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves plugin steps for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing plugin steps for the table</returns>
    [McpServerTool, Description("Retrieves all plugin steps registered for a specific table in Dataverse.")]
    public static async Task<string> ReadPluginSteps(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient(); var stepQuery = new QueryExpression("sdkmessageprocessingstep")
            {
                ColumnSet = new ColumnSet(
                    "sdkmessageprocessingstepid", "name", "description", "stage", "mode",
                    "rank", "statuscode", "filteringattributes", "impersonatinguserid",
                    "sdkmessageid", "plugintypeid", "createdon", "modifiedon"),
                Orders = { new OrderExpression("rank", OrderType.Ascending) }
            };

            // Add link to sdkmessagefilter to filter by entity
            var messageFilterLink = stepQuery.AddLink("sdkmessagefilter", "sdkmessagefilterid", "sdkmessagefilterid");
            messageFilterLink.LinkCriteria = new FilterExpression
            {
                Conditions = { new ConditionExpression("primaryobjecttypecode", ConditionOperator.Equal, tableName) }
            };

            // Add linked entities to get message and plugin type names
            var messageLink = stepQuery.AddLink("sdkmessage", "sdkmessageid", "sdkmessageid");
            messageLink.Columns = new ColumnSet("name");
            messageLink.EntityAlias = "message";

            var pluginTypeLink = stepQuery.AddLink("plugintype", "plugintypeid", "plugintypeid");
            pluginTypeLink.Columns = new ColumnSet("name", "typename");
            pluginTypeLink.EntityAlias = "plugintype";

            var result = await serviceClient.RetrieveMultipleAsync(stepQuery);

            var steps = result.Entities.Select(e => new
            {
                StepId = e.GetAttributeValue<Guid>("sdkmessageprocessingstepid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                MessageName = e.GetAttributeValue<AliasedValue>("message.name")?.Value?.ToString(),
                PluginTypeName = e.GetAttributeValue<AliasedValue>("plugintype.name")?.Value?.ToString(),
                PluginTypeFullName = e.GetAttributeValue<AliasedValue>("plugintype.typename")?.Value?.ToString(),
                Stage = GetStageText(e.GetAttributeValue<OptionSetValue>("stage")?.Value),
                Mode = GetModeText(e.GetAttributeValue<OptionSetValue>("mode")?.Value),
                Rank = e.GetAttributeValue<int?>("rank"),
                Status = GetStatusText(e.GetAttributeValue<OptionSetValue>("statuscode")?.Value),
                FilteringAttributes = e.GetAttributeValue<string>("filteringattributes"),
                ImpersonatingUserId = e.GetAttributeValue<EntityReference>("impersonatinguserid")?.Id,
                CreatedOn = e.GetAttributeValue<DateTime?>("createdon"),
                ModifiedOn = e.GetAttributeValue<DateTime?>("modifiedon")
            }).ToList();

            return JsonSerializer.Serialize(steps, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving plugin steps: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all custom APIs in the environment
    /// </summary>
    /// <returns>JSON string containing custom APIs information</returns>
    [McpServerTool, Description("Retrieves all custom API definitions in the Dataverse environment.")]
    public static async Task<string> ReadCustomApis()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var customApiQuery = new QueryExpression("customapi")
            {
                ColumnSet = new ColumnSet(
                    "customapiid", "uniquename", "displayname", "description", "bindingtype",
                    "boundentitylogicalname", "isfunction", "isprivate", "allowedcustomprocessingsteptype",
                    "createdon", "modifiedon", "executeprivilegename"
                ),
                Orders = { new OrderExpression("displayname", OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(customApiQuery);

            var customApis = result.Entities.Select(e => new
            {
                CustomApiId = e.GetAttributeValue<Guid>("customapiid"),
                UniqueName = e.GetAttributeValue<string>("uniquename"),
                DisplayName = e.GetAttributeValue<string>("displayname"),
                Description = e.GetAttributeValue<string>("description"),
                BindingType = GetBindingTypeText(e.GetAttributeValue<OptionSetValue>("bindingtype")?.Value),
                BoundEntityLogicalName = e.GetAttributeValue<string>("boundentitylogicalname"),
                IsFunction = e.GetAttributeValue<bool>("isfunction"),
                IsPrivate = e.GetAttributeValue<bool>("isprivate"),
                AllowedCustomProcessingStepType = GetCustomProcessingStepTypeText(e.GetAttributeValue<OptionSetValue>("allowedcustomprocessingsteptype")?.Value),
                ExecutePrivilegeName = e.GetAttributeValue<string>("executeprivilegename"),
                CreatedOn = e.GetAttributeValue<DateTime?>("createdon"),
                ModifiedOn = e.GetAttributeValue<DateTime?>("modifiedon")
            }).ToList();

            return JsonSerializer.Serialize(customApis, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving custom APIs: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific custom API
    /// </summary>
    /// <param name="uniqueName">The unique name of the custom API</param>
    /// <returns>JSON string containing detailed custom API information including parameters</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific custom API including request and response parameters.")]
    public static async Task<string> ReadCustomApiDetails(string uniqueName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Get custom API details
            var customApiQuery = new QueryExpression("customapi")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName) }
                }
            };

            var customApiResult = await serviceClient.RetrieveMultipleAsync(customApiQuery);

            if (customApiResult.Entities.Count == 0)
            {
                return $"Custom API '{uniqueName}' not found.";
            }

            var customApi = customApiResult.Entities[0];
            var customApiId = customApi.GetAttributeValue<Guid>("customapiid");

            // Get request parameters
            var requestParamQuery = new QueryExpression("customapirequestparameter")
            {
                ColumnSet = new ColumnSet(
                    "customapirequestparameterid", "uniquename", "displayname", "description",
                    "type", "isoptional", "logicalentityname", "createdon"
                ),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("customapiid", ConditionOperator.Equal, customApiId) }
                },
                Orders = { new OrderExpression("uniquename", OrderType.Ascending) }
            };

            var requestParamResult = await serviceClient.RetrieveMultipleAsync(requestParamQuery);

            // Get response properties
            var responseParamQuery = new QueryExpression("customapiresponseproperty")
            {
                ColumnSet = new ColumnSet(
                    "customapiresponsepropertyid", "uniquename", "displayname", "description",
                    "type", "logicalentityname", "createdon"
                ),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("customapiid", ConditionOperator.Equal, customApiId) }
                },
                Orders = { new OrderExpression("uniquename", OrderType.Ascending) }
            };

            var responseParamResult = await serviceClient.RetrieveMultipleAsync(responseParamQuery);

            var customApiDetails = new
            {
                CustomApiId = customApi.GetAttributeValue<Guid>("customapiid"),
                UniqueName = customApi.GetAttributeValue<string>("uniquename"),
                DisplayName = customApi.GetAttributeValue<string>("displayname"),
                Description = customApi.GetAttributeValue<string>("description"),
                BindingType = GetBindingTypeText(customApi.GetAttributeValue<OptionSetValue>("bindingtype")?.Value),
                BoundEntityLogicalName = customApi.GetAttributeValue<string>("boundentitylogicalname"),
                IsFunction = customApi.GetAttributeValue<bool>("isfunction"),
                IsPrivate = customApi.GetAttributeValue<bool>("isprivate"),
                AllowedCustomProcessingStepType = GetCustomProcessingStepTypeText(customApi.GetAttributeValue<OptionSetValue>("allowedcustomprocessingsteptype")?.Value),
                ExecutePrivilegeName = customApi.GetAttributeValue<string>("executeprivilegename"),
                CreatedOn = customApi.GetAttributeValue<DateTime?>("createdon"),
                ModifiedOn = customApi.GetAttributeValue<DateTime?>("modifiedon"),
                RequestParameters = requestParamResult.Entities.Select(p => new
                {
                    ParameterId = p.GetAttributeValue<Guid>("customapirequestparameterid"),
                    UniqueName = p.GetAttributeValue<string>("uniquename"),
                    DisplayName = p.GetAttributeValue<string>("displayname"),
                    Description = p.GetAttributeValue<string>("description"),
                    Type = GetParameterTypeText(p.GetAttributeValue<OptionSetValue>("type")?.Value),
                    IsOptional = p.GetAttributeValue<bool>("isoptional"),
                    LogicalEntityName = p.GetAttributeValue<string>("logicalentityname")
                }).ToList(),
                ResponseProperties = responseParamResult.Entities.Select(p => new
                {
                    PropertyId = p.GetAttributeValue<Guid>("customapiresponsepropertyid"),
                    UniqueName = p.GetAttributeValue<string>("uniquename"),
                    DisplayName = p.GetAttributeValue<string>("displayname"),
                    Description = p.GetAttributeValue<string>("description"),
                    Type = GetParameterTypeText(p.GetAttributeValue<OptionSetValue>("type")?.Value),
                    LogicalEntityName = p.GetAttributeValue<string>("logicalentityname")
                }).ToList()
            };

            return JsonSerializer.Serialize(customApiDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving custom API details: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all plugin types in the environment
    /// </summary>
    /// <returns>JSON string containing plugin types information</returns>
    [McpServerTool, Description("Retrieves all plugin types registered in the Dataverse environment.")]
    public static async Task<string> ReadPluginTypes()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var pluginTypeQuery = new QueryExpression("plugintype")
            {
                ColumnSet = new ColumnSet(
                    "plugintypeid", "name", "typename", "friendlyname", "description",
                    "pluginassemblyid", "createdon", "modifiedon", "isworkflowactivity"
                ),
                Orders = { new OrderExpression("name", OrderType.Ascending) }
            };

            // Add linked entity to get assembly name
            var assemblyLink = pluginTypeQuery.AddLink("pluginassembly", "pluginassemblyid", "pluginassemblyid");
            assemblyLink.Columns = new ColumnSet("name", "version");
            assemblyLink.EntityAlias = "assembly";

            var result = await serviceClient.RetrieveMultipleAsync(pluginTypeQuery);

            var pluginTypes = result.Entities.Select(e => new
            {
                PluginTypeId = e.GetAttributeValue<Guid>("plugintypeid"),
                Name = e.GetAttributeValue<string>("name"),
                TypeName = e.GetAttributeValue<string>("typename"),
                FriendlyName = e.GetAttributeValue<string>("friendlyname"),
                Description = e.GetAttributeValue<string>("description"),
                AssemblyName = e.GetAttributeValue<AliasedValue>("assembly.name")?.Value?.ToString(),
                AssemblyVersion = e.GetAttributeValue<AliasedValue>("assembly.version")?.Value?.ToString(),
                IsWorkflowActivity = e.GetAttributeValue<bool>("isworkflowactivity"),
                CreatedOn = e.GetAttributeValue<DateTime?>("createdon"),
                ModifiedOn = e.GetAttributeValue<DateTime?>("modifiedon")
            }).ToList();

            return JsonSerializer.Serialize(pluginTypes, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving plugin types: {ex.Message}";
        }
    }

    #endregion

    #region Helper Methods for Plugin and Custom API

    private static string GetIsolationModeText(int? isolationMode)
    {
        return isolationMode switch
        {
            1 => "None",
            2 => "Sandbox",
            3 => "External",
            _ => $"Unknown ({isolationMode})"
        };
    }

    private static string GetSourceTypeText(int? sourceType)
    {
        return sourceType switch
        {
            0 => "Database",
            1 => "Disk",
            2 => "Normal",
            3 => "AzureWebApp",
            _ => $"Unknown ({sourceType})"
        };
    }

    private static string GetStageText(int? stage)
    {
        return stage switch
        {
            10 => "Pre-validation",
            20 => "Pre-operation",
            40 => "Post-operation",
            50 => "Post-operation (Deprecated)",
            _ => $"Unknown ({stage})"
        };
    }

    private static string GetModeText(int? mode)
    {
        return mode switch
        {
            0 => "Synchronous",
            1 => "Asynchronous",
            _ => $"Unknown ({mode})"
        };
    }

    private static string GetStatusText(int? status)
    {
        return status switch
        {
            1 => "Enabled",
            2 => "Disabled",
            _ => $"Unknown ({status})"
        };
    }

    private static string GetBindingTypeText(int? bindingType)
    {
        return bindingType switch
        {
            0 => "Global",
            1 => "Entity",
            2 => "EntityCollection",
            _ => $"Unknown ({bindingType})"
        };
    }

    private static string GetCustomProcessingStepTypeText(int? stepType)
    {
        return stepType switch
        {
            0 => "None",
            1 => "AsyncOnly",
            2 => "SyncAndAsync",
            _ => $"Unknown ({stepType})"
        };
    }

    private static string GetParameterTypeText(int? parameterType)
    {
        return parameterType switch
        {
            0 => "Boolean",
            1 => "DateTime",
            2 => "Decimal",
            3 => "Entity",
            4 => "EntityCollection",
            5 => "EntityReference",
            6 => "Float",
            7 => "Integer",
            8 => "Money",
            9 => "Picklist",
            10 => "String",
            11 => "StringArray",
            12 => "Guid",
            _ => $"Unknown ({parameterType})"
        };
    }

    #endregion
}
