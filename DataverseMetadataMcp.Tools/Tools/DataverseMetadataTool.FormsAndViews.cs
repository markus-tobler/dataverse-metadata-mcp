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
    #region Forms and Views Metadata Methods

    /// <summary>
    /// Retrieves all forms for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing all forms for the table</returns>
    [McpServerTool, Description("Retrieves all forms for a specific table from Dataverse.")]
    public static async Task<string> ReadEntityForms(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("systemform")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("formid", "name", "description", "type", "istabletenabled", "ismanaged", "formactivationstate", "objecttypecode"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("objecttypecode", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, tableName)
                    }
                },
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var forms = result.Entities.Select(e => new
            {
                FormId = e.GetAttributeValue<Guid>("formid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                Type = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("type")?.Value,
                TypeName = GetFormTypeName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("type")?.Value ?? 0),
                IsTabletEnabled = e.GetAttributeValue<bool>("istabletenabled"),
                IsManaged = e.GetAttributeValue<bool>("ismanaged"),
                FormActivationState = e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("formactivationstate")?.Value,
                ActivationStateName = GetFormActivationStateName(e.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("formactivationstate")?.Value ?? 0),
                ObjectTypeCode = e.GetAttributeValue<string>("objecttypecode")
            }).ToList();

            return JsonSerializer.Serialize(forms, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving entity forms: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all views for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing all views for the table</returns>
    [McpServerTool, Description("Retrieves all views for a specific table from Dataverse.")]
    public static async Task<string> ReadEntityViews(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("savedquery")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("savedqueryid", "name", "description", "querytype", "isdefault", "ismanaged", "isquickfindquery", "isprivate", "returnedtypecode"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("returnedtypecode", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, tableName)
                    }
                },
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var views = result.Entities.Select(e => new
            {
                SavedQueryId = e.GetAttributeValue<Guid>("savedqueryid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                QueryType = e.GetAttributeValue<int>("querytype"),
                QueryTypeName = GetQueryTypeName(e.GetAttributeValue<int>("querytype")),
                IsDefault = e.GetAttributeValue<bool>("isdefault"),
                IsManaged = e.GetAttributeValue<bool>("ismanaged"),
                IsQuickFindQuery = e.GetAttributeValue<bool>("isquickfindquery"),
                IsPrivate = e.GetAttributeValue<bool>("isprivate"),
                ReturnedTypeCode = e.GetAttributeValue<string>("returnedtypecode")
            }).ToList();

            return JsonSerializer.Serialize(views, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving entity views: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific form
    /// </summary>
    /// <param name="formId">The ID of the form</param>
    /// <returns>JSON string containing detailed form information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific form from Dataverse.")]
    public static async Task<string> ReadFormDetails(string formId)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            if (!Guid.TryParse(formId, out var formGuid))
            {
                return JsonSerializer.Serialize(new { Error = "Invalid form ID format" }, new JsonSerializerOptions { WriteIndented = true });
            }

            var form = await serviceClient.RetrieveAsync("systemform", formGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(
                "formid", "name", "description", "type", "objecttypecode", "formxml", "istabletenabled",
                "ismanaged", "formactivationstate", "version", "introducedversion"));

            var formDetails = new
            {
                FormId = form.GetAttributeValue<Guid>("formid"),
                Name = form.GetAttributeValue<string>("name"),
                Description = form.GetAttributeValue<string>("description"),
                Type = form.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("type")?.Value,
                TypeName = GetFormTypeName(form.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("type")?.Value ?? 0),
                ObjectTypeCode = form.GetAttributeValue<string>("objecttypecode"),
                IsTabletEnabled = form.GetAttributeValue<bool>("istabletenabled"),
                IsManaged = form.GetAttributeValue<bool>("ismanaged"),
                FormActivationState = form.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("formactivationstate")?.Value,
                ActivationStateName = GetFormActivationStateName(form.GetAttributeValue<Microsoft.Xrm.Sdk.OptionSetValue>("formactivationstate")?.Value ?? 0),
                Version = form.GetAttributeValue<string>("version"),
                IntroducedVersion = form.GetAttributeValue<string>("introducedversion"),
                FormXmlLength = form.GetAttributeValue<string>("formxml")?.Length ?? 0,
                HasFormXml = !string.IsNullOrEmpty(form.GetAttributeValue<string>("formxml"))
            };

            return JsonSerializer.Serialize(formDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving form details: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific view
    /// </summary>
    /// <param name="viewId">The ID of the view</param>
    /// <returns>JSON string containing detailed view information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific view from Dataverse.")]
    public static async Task<string> ReadViewDetails(string viewId)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            if (!Guid.TryParse(viewId, out var viewGuid))
            {
                return JsonSerializer.Serialize(new { Error = "Invalid view ID format" }, new JsonSerializerOptions { WriteIndented = true });
            }

            var view = await serviceClient.RetrieveAsync("savedquery", viewGuid, new Microsoft.Xrm.Sdk.Query.ColumnSet(
                "savedqueryid", "name", "description", "querytype", "returnedtypecode", "fetchxml",
                "layoutxml", "columnsetxml", "isdefault", "ismanaged", "isquickfindquery", "isprivate"));

            var viewDetails = new
            {
                SavedQueryId = view.GetAttributeValue<Guid>("savedqueryid"),
                Name = view.GetAttributeValue<string>("name"),
                Description = view.GetAttributeValue<string>("description"),
                QueryType = view.GetAttributeValue<int>("querytype"),
                QueryTypeName = GetQueryTypeName(view.GetAttributeValue<int>("querytype")),
                ReturnedTypeCode = view.GetAttributeValue<string>("returnedtypecode"),
                IsDefault = view.GetAttributeValue<bool>("isdefault"),
                IsManaged = view.GetAttributeValue<bool>("ismanaged"),
                IsQuickFindQuery = view.GetAttributeValue<bool>("isquickfindquery"),
                IsPrivate = view.GetAttributeValue<bool>("isprivate"),
                FetchXmlLength = view.GetAttributeValue<string>("fetchxml")?.Length ?? 0,
                LayoutXmlLength = view.GetAttributeValue<string>("layoutxml")?.Length ?? 0,
                ColumnSetXmlLength = view.GetAttributeValue<string>("columnsetxml")?.Length ?? 0,
                HasFetchXml = !string.IsNullOrEmpty(view.GetAttributeValue<string>("fetchxml")),
                HasLayoutXml = !string.IsNullOrEmpty(view.GetAttributeValue<string>("layoutxml")),
                HasColumnSetXml = !string.IsNullOrEmpty(view.GetAttributeValue<string>("columnsetxml"))
            };

            return JsonSerializer.Serialize(viewDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving view details: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a human-readable name for form type values
    /// </summary>
    /// <param name="formType">The form type value</param>
    /// <returns>A descriptive string of the form type</returns>
    private static string GetFormTypeName(int formType)
    {
        return formType switch
        {
            1 => "Create",
            2 => "Update",
            3 => "ReadOnly",
            4 => "Admin",
            5 => "BulkEdit",
            6 => "ReadOnlyGrid",
            7 => "Associated",
            8 => "QuickViewForm",
            9 => "QuickCreate",
            10 => "Dialog",
            11 => "TaskBasedFlow",
            12 => "InteractionCentricDashboard",
            13 => "Card",
            14 => "Main - Interactive experience",
            15 => "ContextualDashboard",
            16 => "Other",
            17 => "MainBackup",
            18 => "AppointmentBook",
            19 => "Mobile - Express",
            _ => $"Unknown ({formType})"
        };
    }

    /// <summary>
    /// Gets a human-readable name for form activation state values
    /// </summary>
    /// <param name="activationState">The activation state value</param>
    /// <returns>A descriptive string of the activation state</returns>
    private static string GetFormActivationStateName(int activationState)
    {
        return activationState switch
        {
            0 => "Inactive",
            1 => "Active",
            _ => $"Unknown ({activationState})"
        };
    }

    /// <summary>
    /// Gets a human-readable name for query type values
    /// </summary>
    /// <param name="queryType">The query type value</param>
    /// <returns>A descriptive string of the query type</returns>
    private static string GetQueryTypeName(int queryType)
    {
        return queryType switch
        {
            0 => "MainApplicationView",
            1 => "AdvancedFind",
            2 => "SubGrid",
            4 => "QuickFind",
            8 => "Lookup",
            16 => "ReportingView",
            32 => "Other",
            64 => "MainApplicationViewWithoutSubAreas",
            128 => "SavedQueryVisualization",
            256 => "UserQueryVisualization",
            512 => "Interactive",
            1024 => "Dashboard",
            2048 => "CustomSearch",
            _ => $"Unknown ({queryType})"
        };
    }

    #endregion
}