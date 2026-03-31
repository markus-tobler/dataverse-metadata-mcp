using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using ModelContextProtocol.Server;
using DataverseMetadataMcp.Tools.Configuration;
using System.ComponentModel;
using System.Security;
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
    /// <param name="includeXml">When true, includes the full FormXml in the response</param>
    /// <returns>JSON string containing detailed form information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific form from Dataverse. By default returns summary fields only; set includeXml=true to include the full FormXml.")]
    public static async Task<string> ReadFormDetails(
        [Description("The GUID of the form to retrieve.")] string formId,
        [Description("When true, includes the full FormXml in the response. Defaults to false.")] bool includeXml = false)
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

            var formXml = form.GetAttributeValue<string>("formxml");

            if (includeXml)
            {
                var formDetailsWithXml = new
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
                    FormXml = formXml
                };
                return JsonSerializer.Serialize(formDetailsWithXml, new JsonSerializerOptions { WriteIndented = true });
            }

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
                FormXmlLength = formXml?.Length ?? 0,
                HasFormXml = !string.IsNullOrEmpty(formXml)
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
    /// <param name="includeXml">When true, includes full FetchXml, LayoutXml, and ColumnSetXml in the response</param>
    /// <returns>JSON string containing detailed view information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific view from Dataverse. By default returns summary fields only; set includeXml=true to include the full FetchXml, LayoutXml, and ColumnSetXml.")]
    public static async Task<string> ReadViewDetails(
        [Description("The GUID of the view to retrieve.")] string viewId,
        [Description("When true, includes the full FetchXml, LayoutXml, and ColumnSetXml in the response. Defaults to false.")] bool includeXml = false)
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

            var fetchXml = view.GetAttributeValue<string>("fetchxml");
            var layoutXml = view.GetAttributeValue<string>("layoutxml");
            var columnSetXml = view.GetAttributeValue<string>("columnsetxml");

            if (includeXml)
            {
                var viewDetailsWithXml = new
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
                    FetchXml = fetchXml,
                    LayoutXml = layoutXml,
                    ColumnSetXml = columnSetXml
                };
                return JsonSerializer.Serialize(viewDetailsWithXml, new JsonSerializerOptions { WriteIndented = true });
            }

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
                FetchXmlLength = fetchXml?.Length ?? 0,
                HasFetchXml = !string.IsNullOrEmpty(fetchXml),
                LayoutXmlLength = layoutXml?.Length ?? 0,
                HasLayoutXml = !string.IsNullOrEmpty(layoutXml),
                ColumnSetXmlLength = columnSetXml?.Length ?? 0,
                HasColumnSetXml = !string.IsNullOrEmpty(columnSetXml)
            };

            return JsonSerializer.Serialize(viewDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving view details: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates a new system view (savedquery) for a specific table in Dataverse.
    /// </summary>
    [McpServerTool, Description("Creates a new system view for a specific table in Dataverse. Provide FetchXML for the query and LayoutXML for the column layout. QueryType: 0=MainApplicationView, 1=AdvancedFind, 2=SubGrid, 4=QuickFind, 8=Lookup.")]
    public static async Task<string> CreateView(
        [Description("The logical name of the table the view belongs to (e.g. 'account').")] string tableName,
        [Description("The display name of the new view.")] string viewName,
        [Description("The FetchXML query that defines which records and columns to retrieve.")] string fetchXml,
        [Description("The LayoutXML that defines the column layout for the view.")] string layoutXml,
        [Description("Optional description of the view.")] string? description = null,
        [Description("Optional query type. Defaults to 0 (MainApplicationView).")] int queryType = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return JsonSerializer.Serialize(new { Error = "tableName is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });
            if (string.IsNullOrWhiteSpace(viewName))
                return JsonSerializer.Serialize(new { Error = "viewName is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });
            if (string.IsNullOrWhiteSpace(fetchXml))
                return JsonSerializer.Serialize(new { Error = "fetchXml is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });
            if (string.IsNullOrWhiteSpace(layoutXml))
                return JsonSerializer.Serialize(new { Error = "layoutXml is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });

            var serviceClient = ConfigurationHelper.GetServiceClient();

            var entity = new Microsoft.Xrm.Sdk.Entity("savedquery")
            {
                ["name"] = viewName,
                ["returnedtypecode"] = tableName,
                ["fetchxml"] = fetchXml,
                ["layoutxml"] = layoutXml,
                ["querytype"] = queryType
            };

            if (description != null)
                entity["description"] = description;

            var viewId = await serviceClient.CreateAsync(entity);

            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><entities><entity>{SecurityElement.Escape(tableName)}</entity></entities></importexportxml>"
            };
            await serviceClient.ExecuteAsync(publishRequest);

            return JsonSerializer.Serialize(new
            {
                Success = true,
                SavedQueryId = viewId,
                Name = viewName,
                TableName = tableName,
                QueryType = queryType,
                QueryTypeName = GetQueryTypeName(queryType),
                Message = "View created and published successfully."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error creating view: {ex.Message}";
        }
    }

    /// <summary>
    /// Updates an existing system view (savedquery) in Dataverse.
    /// </summary>
    [McpServerTool, Description("Updates an existing system view in Dataverse. Only the provided parameters will be updated. Call ReadViewDetails first to retrieve the current FetchXML and LayoutXML before making modifications.")]
    public static async Task<string> UpdateView(
        [Description("The GUID of the view to update.")] string viewId,
        [Description("New display name for the view.")] string? name = null,
        [Description("New description for the view.")] string? description = null,
        [Description("New FetchXML query for the view.")] string? fetchXml = null,
        [Description("New LayoutXML for the view column layout.")] string? layoutXml = null)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            if (!Guid.TryParse(viewId, out var viewGuid))
                return JsonSerializer.Serialize(new { Error = "Invalid view ID format" }, new JsonSerializerOptions { WriteIndented = true });

            if (name == null && description == null && fetchXml == null && layoutXml == null)
                return JsonSerializer.Serialize(new { Error = "At least one field to update must be provided (name, description, fetchXml, or layoutXml)." }, new JsonSerializerOptions { WriteIndented = true });

            // Retrieve existing view to get the table name for publishing
            var existing = await serviceClient.RetrieveAsync("savedquery", viewGuid,
                new Microsoft.Xrm.Sdk.Query.ColumnSet("returnedtypecode"));
            var tableName = existing.GetAttributeValue<string>("returnedtypecode");

            var entity = new Microsoft.Xrm.Sdk.Entity("savedquery") { Id = viewGuid };

            if (name != null)
                entity["name"] = name;
            if (description != null)
                entity["description"] = description;
            if (fetchXml != null)
                entity["fetchxml"] = fetchXml;
            if (layoutXml != null)
                entity["layoutxml"] = layoutXml;

            await serviceClient.UpdateAsync(entity);

            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><entities><entity>{SecurityElement.Escape(tableName)}</entity></entities></importexportxml>"
            };
            await serviceClient.ExecuteAsync(publishRequest);

            var updatedFields = new List<string>();
            if (name != null) updatedFields.Add("Name");
            if (description != null) updatedFields.Add("Description");
            if (fetchXml != null) updatedFields.Add("FetchXml");
            if (layoutXml != null) updatedFields.Add("LayoutXml");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                SavedQueryId = viewGuid,
                UpdatedFields = updatedFields,
                Message = "View updated and published successfully."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error updating view: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates a new system form for a specific table in Dataverse.
    /// </summary>
    [McpServerTool, Description("Creates a new system form for a specific table in Dataverse. Provide FormXML for the layout. FormType: 2=Main (default), 6=QuickViewForm, 7=QuickCreate. Call ReadFormDetails with includeXml=true on an existing form to get a FormXML template.")]
    public static async Task<string> CreateForm(
        [Description("The logical name of the table the form belongs to (e.g. 'account').")] string tableName,
        [Description("The display name of the new form.")] string formName,
        [Description("The FormXML that defines the form layout. Must be a valid XML string with a <form> root element.")] string formXml,
        [Description("Optional description of the form.")] string? description = null,
        [Description("Form type integer. Defaults to 2 (Main). Common values: 2=Main, 6=QuickViewForm, 7=QuickCreate.")] int formType = 2,
        [Description("Whether the form is enabled for desktop. Defaults to true.")] bool isDesktopEnabled = true,
        [Description("Whether the form is enabled for tablets. Defaults to false.")] bool isTabletEnabled = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return JsonSerializer.Serialize(new { Error = "tableName is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });
            if (string.IsNullOrWhiteSpace(formName))
                return JsonSerializer.Serialize(new { Error = "formName is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });
            if (string.IsNullOrWhiteSpace(formXml))
                return JsonSerializer.Serialize(new { Error = "formXml is required and cannot be empty." }, new JsonSerializerOptions { WriteIndented = true });

            var serviceClient = ConfigurationHelper.GetServiceClient();

            var entity = new Microsoft.Xrm.Sdk.Entity("systemform")
            {
                ["name"] = formName,
                ["objecttypecode"] = tableName,
                ["formxml"] = formXml,
                ["type"] = new Microsoft.Xrm.Sdk.OptionSetValue(formType),
                ["isdesktopenabled"] = isDesktopEnabled,
                ["istabletenabled"] = isTabletEnabled
            };

            if (description != null)
                entity["description"] = description;

            var formId = await serviceClient.CreateAsync(entity);

            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><entities><entity>{SecurityElement.Escape(tableName)}</entity></entities></importexportxml>"
            };
            await serviceClient.ExecuteAsync(publishRequest);

            return JsonSerializer.Serialize(new
            {
                Success = true,
                FormId = formId,
                Name = formName,
                TableName = tableName,
                FormType = formType,
                FormTypeName = GetFormTypeName(formType),
                Message = "Form created and published successfully."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error creating form: {ex.Message}";
        }
    }

    /// <summary>
    /// Updates an existing system form in Dataverse.
    /// </summary>
    [McpServerTool, Description("Updates an existing system form in Dataverse. Only the provided parameters will be updated. Call ReadFormDetails with includeXml=true first to retrieve the current FormXml before making modifications.")]
    public static async Task<string> UpdateForm(
        [Description("The GUID of the form to update.")] string formId,
        [Description("New display name for the form.")] string? name = null,
        [Description("New description for the form.")] string? description = null,
        [Description("New FormXML for the form layout.")] string? formXml = null)
    {
        try
        {
            if (!Guid.TryParse(formId, out var formGuid))
                return JsonSerializer.Serialize(new { Error = "Invalid form ID format" }, new JsonSerializerOptions { WriteIndented = true });

            if (name == null && description == null && formXml == null)
                return JsonSerializer.Serialize(new { Error = "At least one field to update must be provided (name, description, or formXml)." }, new JsonSerializerOptions { WriteIndented = true });

            var serviceClient = ConfigurationHelper.GetServiceClient();
            // Retrieve existing form to get the table name for publishing
            var existing = await serviceClient.RetrieveAsync("systemform", formGuid,
                new Microsoft.Xrm.Sdk.Query.ColumnSet("objecttypecode"));
            var tableName = existing.GetAttributeValue<string>("objecttypecode");

            var entity = new Microsoft.Xrm.Sdk.Entity("systemform") { Id = formGuid };

            if (name != null)
                entity["name"] = name;
            if (description != null)
                entity["description"] = description;
            if (formXml != null)
                entity["formxml"] = formXml;

            await serviceClient.UpdateAsync(entity);

            var publishRequest = new PublishXmlRequest
            {
                ParameterXml = $"<importexportxml><entities><entity>{SecurityElement.Escape(tableName)}</entity></entities></importexportxml>"
            };
            await serviceClient.ExecuteAsync(publishRequest);

            var updatedFields = new List<string>();
            if (name != null) updatedFields.Add("Name");
            if (description != null) updatedFields.Add("Description");
            if (formXml != null) updatedFields.Add("FormXml");

            return JsonSerializer.Serialize(new
            {
                Success = true,
                FormId = formGuid,
                UpdatedFields = updatedFields,
                Message = "Form updated and published successfully."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error updating form: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a human-readable name for form type values
    /// </summary>
    /// <param name="formType">The form type value</param>
    /// <returns>A descriptive string of the form type</returns>
    private static string GetFormTypeName(int formType) => formType switch
    {
        0 => "Dashboard",
        1 => "AppointmentBook",
        2 => "Main",
        3 => "MiniCampaignBO",
        4 => "Preview",
        5 => "Mobile - Express",
        6 => "QuickViewForm",
        7 => "QuickCreate",
        8 => "Dialog",
        9 => "TaskFlowForm",
        10 => "InteractionCentricDashboard",
        11 => "Card",
        12 => "Main - Interactive experience",
        13 => "ContextualDashboard",
        100 => "Other",
        101 => "MainBackup",
        102 => "AppointmentBookBackup",
        103 => "Power BI Dashboard",
        _ => $"Unknown ({formType})"
    };

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