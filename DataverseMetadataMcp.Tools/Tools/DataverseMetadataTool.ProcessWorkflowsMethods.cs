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
    #region Process and Workflows Metadata Methods

    /// <summary>
    /// Retrieves all business process flows from Dataverse
    /// </summary>
    /// <returns>JSON string containing all business process flows</returns>
    [McpServerTool, Description("Retrieves all business process flows from Dataverse.")]
    public static async Task<string> ReadBusinessProcessFlows()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("workflow")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("workflowid", "name", "description", "category", "primaryentity", "statecode", "statuscode", "ismanaged", "createdon", "modifiedon"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("category", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, 4) // Business Process Flow
                    }
                },
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var businessProcessFlows = result.Entities.Select(e => new
            {
                WorkflowId = e.GetAttributeValue<Guid>("workflowid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                Category = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("category")?.Value,
                CategoryName = GetWorkflowCategoryName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("category")?.Value ?? 0),
                PrimaryEntity = e.GetAttributeValue<string>("primaryentity"),
                StateCode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode")?.Value,
                StateName = GetWorkflowStateName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode")?.Value ?? 0),
                StatusCode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode")?.Value,
                IsManaged = e.GetAttributeValue<bool>("ismanaged"),
                CreatedOn = e.GetAttributeValue<DateTime>("createdon"),
                ModifiedOn = e.GetAttributeValue<DateTime>("modifiedon")
            }).ToList();

            return JsonSerializer.Serialize(businessProcessFlows, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving business process flows: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves workflows for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing workflows for the table</returns>
    [McpServerTool, Description("Retrieves workflows for a specific table from Dataverse.")]
    public static async Task<string> ReadWorkflows(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("workflow")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("workflowid", "name", "description", "category", "primaryentity", "mode", "scope", "triggeronactivate", "triggeroncreate", "triggerondelete", "triggeronupdateattributelist", "statecode", "statuscode", "ismanaged"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("primaryentity", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, tableName),
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("category", Microsoft.Xrm.Sdk.Query.ConditionOperator.In, new object[] { 0, 1, 2, 3, 5 }) // Exclude BPF (4)
                    }
                },
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var workflows = result.Entities.Select(e => new
            {
                WorkflowId = e.GetAttributeValue<Guid>("workflowid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                Category = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("category")?.Value,
                CategoryName = GetWorkflowCategoryName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("category")?.Value ?? 0),
                PrimaryEntity = e.GetAttributeValue<string>("primaryentity"),
                Mode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("mode")?.Value,
                ModeName = GetWorkflowModeName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("mode")?.Value ?? 0),
                Scope = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("scope")?.Value,
                ScopeName = GetWorkflowScopeName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("scope")?.Value ?? 0),
                TriggerOnActivate = e.GetAttributeValue<bool>("triggeronactivate"),
                TriggerOnCreate = e.GetAttributeValue<bool>("triggeroncreate"),
                TriggerOnDelete = e.GetAttributeValue<bool>("triggerondelete"),
                TriggerOnUpdateAttributeList = e.GetAttributeValue<string>("triggeronupdateattributelist"),
                StateCode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode")?.Value,
                StateName = GetWorkflowStateName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode")?.Value ?? 0),
                StatusCode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode")?.Value,
                IsManaged = e.GetAttributeValue<bool>("ismanaged")
            }).ToList();

            return JsonSerializer.Serialize(workflows, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving workflows: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves business rules for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing business rules for the table</returns>
    [McpServerTool, Description("Retrieves business rules for a specific table from Dataverse.")]
    public static async Task<string> ReadBusinessRules(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("workflow")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("workflowid", "name", "description", "category", "primaryentity", "clientdata", "statecode", "statuscode", "ismanaged", "createdon", "modifiedon"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("primaryentity", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, tableName),
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("category", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, 2) // Business Rule
                    }
                },
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var businessRules = result.Entities.Select(e => new
            {
                WorkflowId = e.GetAttributeValue<Guid>("workflowid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                Category = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("category")?.Value,
                CategoryName = GetWorkflowCategoryName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("category")?.Value ?? 0),
                PrimaryEntity = e.GetAttributeValue<string>("primaryentity"),
                HasClientData = !string.IsNullOrEmpty(e.GetAttributeValue<string>("clientdata")),
                ClientDataLength = e.GetAttributeValue<string>("clientdata")?.Length ?? 0,
                StateCode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode")?.Value,
                StateName = GetWorkflowStateName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statecode")?.Value ?? 0),
                StatusCode = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("statuscode")?.Value,
                IsManaged = e.GetAttributeValue<bool>("ismanaged"),
                CreatedOn = e.GetAttributeValue<DateTime>("createdon"),
                ModifiedOn = e.GetAttributeValue<DateTime>("modifiedon")
            }).ToList();

            return JsonSerializer.Serialize(businessRules, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving business rules: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a human-readable name for workflow category values
    /// </summary>
    /// <param name="category">The workflow category value</param>
    /// <returns>A descriptive string of the workflow category</returns>
    private static string GetWorkflowCategoryName(int category)
    {
        return category switch
        {
            0 => "Workflow",
            1 => "Dialog",
            2 => "Business Rule",
            3 => "Action",
            4 => "Business Process Flow",
            5 => "Modern Flow",
            6 => "Desktop Flow",
            _ => $"Unknown ({category})"
        };
    }

    /// <summary>
    /// Gets a human-readable name for workflow mode values
    /// </summary>
    /// <param name="mode">The workflow mode value</param>
    /// <returns>A descriptive string of the workflow mode</returns>
    private static string GetWorkflowModeName(int mode)
    {
        return mode switch
        {
            0 => "Background",
            1 => "Real-time",
            _ => $"Unknown ({mode})"
        };
    }

    /// <summary>
    /// Gets a human-readable name for workflow scope values
    /// </summary>
    /// <param name="scope">The workflow scope value</param>
    /// <returns>A descriptive string of the workflow scope</returns>
    private static string GetWorkflowScopeName(int scope)
    {
        return scope switch
        {
            1 => "User",
            2 => "Business Unit",
            3 => "Parent: Child Business Units",
            4 => "Organization",
            _ => $"Unknown ({scope})"
        };
    }

    /// <summary>
    /// Gets a human-readable name for workflow state values
    /// </summary>
    /// <param name="state">The workflow state value</param>
    /// <returns>A descriptive string of the workflow state</returns>
    private static string GetWorkflowStateName(int state)
    {
        return state switch
        {
            0 => "Draft",
            1 => "Activated",
            2 => "Suspended",
            _ => $"Unknown ({state})"
        };
    }

    #endregion
}