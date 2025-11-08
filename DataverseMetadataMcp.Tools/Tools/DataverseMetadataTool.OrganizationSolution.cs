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
    #region Organization and Solution Metadata Methods

    /// <summary>
    /// Retrieves organization information from Dataverse
    /// </summary>
    /// <returns>JSON string containing organization details</returns>
    [McpServerTool, Description("Retrieves organization information from Dataverse including environment details, languages, and currencies.")]
    public static async Task<string> ReadOrganizationInfo()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var orgQuery = new QueryExpression("organization")
            {
                ColumnSet = new ColumnSet(
                    "organizationid", "name", "friendlyname", "uniquename", "version",
                    "languagecode", "basecurrencyid", "defaultcountryhash", "dateformatstring",
                    "timeformatstring", "weekstartdaycode", "fiscalcalendarstart",
                    "fiscalperiodtype", "fiscalyearformatprefix", "fiscalyearformatsuffix"
                )
            };

            var orgResult = await serviceClient.RetrieveMultipleAsync(orgQuery);

            if (orgResult.Entities.Count == 0)
            {
                return "No organization information found.";
            }

            var org = orgResult.Entities[0];

            // Get base currency details
            var baseCurrencyId = org.GetAttributeValue<EntityReference>("basecurrencyid")?.Id;
            string? baseCurrencyName = null;
            string? baseCurrencySymbol = null;

            if (baseCurrencyId.HasValue)
            {
                try
                {
                    var currencyQuery = new QueryExpression("transactioncurrency")
                    {
                        ColumnSet = new ColumnSet("currencyname", "currencysymbol", "isocurrencycode"),
                        Criteria = new FilterExpression
                        {
                            Conditions = { new ConditionExpression("transactioncurrencyid", ConditionOperator.Equal, baseCurrencyId.Value) }
                        }
                    };

                    var currencyResult = await serviceClient.RetrieveMultipleAsync(currencyQuery);
                    if (currencyResult.Entities.Count > 0)
                    {
                        var currency = currencyResult.Entities[0];
                        baseCurrencyName = currency.GetAttributeValue<string>("currencyname");
                        baseCurrencySymbol = currency.GetAttributeValue<string>("currencysymbol");
                    }
                }
                catch
                {
                    // Ignore currency lookup errors
                }
            }

            var organizationInfo = new
            {
                OrganizationId = org.GetAttributeValue<Guid>("organizationid"),
                Name = org.GetAttributeValue<string>("name"),
                FriendlyName = org.GetAttributeValue<string>("friendlyname"),
                UniqueName = org.GetAttributeValue<string>("uniquename"),
                Version = org.GetAttributeValue<string>("version"),
                LanguageCode = org.GetAttributeValue<int>("languagecode"),
                BaseCurrency = new
                {
                    Id = baseCurrencyId,
                    Name = baseCurrencyName,
                    Symbol = baseCurrencySymbol
                },
                DefaultCountryHash = org.GetAttributeValue<string>("defaultcountryhash"),
                DateFormatString = org.GetAttributeValue<string>("dateformatstring"),
                TimeFormatString = org.GetAttributeValue<string>("timeformatstring"),
                WeekStartDayCode = org.GetAttributeValue<int?>("weekstartdaycode"),
                FiscalCalendarStart = org.GetAttributeValue<DateTime?>("fiscalcalendarstart"),
                FiscalPeriodType = org.GetAttributeValue<int?>("fiscalperiodtype"),
                FiscalYearFormatPrefix = org.GetAttributeValue<string>("fiscalyearformatprefix"),
                FiscalYearFormatSuffix = org.GetAttributeValue<string>("fiscalyearformatsuffix")
            };

            return JsonSerializer.Serialize(organizationInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving organization information: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all installed languages in the environment
    /// </summary>
    /// <returns>JSON string containing installed languages</returns>
    [McpServerTool, Description("Retrieves all installed languages in the Dataverse environment.")]
    public static async Task<string> ReadLanguages()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var langQuery = new QueryExpression("languagelocale")
            {
                ColumnSet = new ColumnSet("languagelocaleid", "name", "code", "localeid", "region"),
                Orders = { new OrderExpression("name", OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(langQuery);

            var languages = result.Entities.Select(e => new
            {
                LanguageLocaleId = e.GetAttributeValue<Guid>("languagelocaleid"),
                Name = e.GetAttributeValue<string>("name"),
                Code = e.GetAttributeValue<string>("code"),
                LocaleId = e.GetAttributeValue<int>("localeid"),
                Region = e.GetAttributeValue<string>("region")
            }).ToList();

            return JsonSerializer.Serialize(languages, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving languages: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all available currencies in the environment
    /// </summary>
    /// <returns>JSON string containing available currencies</returns>
    [McpServerTool, Description("Retrieves all available currencies in the Dataverse environment.")]
    public static async Task<string> ReadCurrencies()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var currencyQuery = new QueryExpression("transactioncurrency")
            {
                ColumnSet = new ColumnSet(
                    "transactioncurrencyid", "currencyname", "currencysymbol",
                    "isocurrencycode", "exchangerate", "currencyprecision", "statuscode"
                ),
                Orders = { new OrderExpression("currencyname", OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(currencyQuery);

            var currencies = result.Entities.Select(e => new
            {
                TransactionCurrencyId = e.GetAttributeValue<Guid>("transactioncurrencyid"),
                CurrencyName = e.GetAttributeValue<string>("currencyname"),
                CurrencySymbol = e.GetAttributeValue<string>("currencysymbol"),
                ISOCurrencyCode = e.GetAttributeValue<string>("isocurrencycode"),
                ExchangeRate = e.GetAttributeValue<decimal?>("exchangerate"),
                CurrencyPrecision = e.GetAttributeValue<int?>("currencyprecision"),
                StatusCode = e.GetAttributeValue<OptionSetValue>("statuscode")?.Value
            }).ToList();

            return JsonSerializer.Serialize(currencies, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving currencies: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all time zones available in the environment
    /// </summary>
    /// <returns>JSON string containing available time zones</returns>
    [McpServerTool, Description("Retrieves all available time zones in the Dataverse environment.")]
    public static async Task<string> ReadTimeZones()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var timezoneQuery = new QueryExpression("timezonedefinition")
            {
                ColumnSet = new ColumnSet(
                    "timezonedefinitionid", "userinterfacename", "standardname",
                    "daylightname", "timezonecode", "bias", "standardbias", "daylightbias"
                ),
                Orders = { new OrderExpression("userinterfacename", OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(timezoneQuery);

            var timeZones = result.Entities.Select(e => new
            {
                TimeZoneDefinitionId = e.GetAttributeValue<Guid>("timezonedefinitionid"),
                UserInterfaceName = e.GetAttributeValue<string>("userinterfacename"),
                StandardName = e.GetAttributeValue<string>("standardname"),
                DaylightName = e.GetAttributeValue<string>("daylightname"),
                TimeZoneCode = e.GetAttributeValue<int?>("timezonecode"),
                Bias = e.GetAttributeValue<int?>("bias"),
                StandardBias = e.GetAttributeValue<int?>("standardbias"),
                DaylightBias = e.GetAttributeValue<int?>("daylightbias")
            }).ToList();

            return JsonSerializer.Serialize(timeZones, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving time zones: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves solutions in the environment
    /// </summary>
    /// <param name="solutionType">The type of solutions to retrieve: 'all', 'managed', or 'unmanaged' (default: 'unmanaged')</param>
    /// <returns>JSON string containing solutions</returns>
    [McpServerTool, Description("Retrieves solutions in the Dataverse environment. Specify 'all', 'managed', or 'unmanaged' (default) to filter by solution type.")]
    public static async Task<string> ReadSolutions(string solutionType = "unmanaged")
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(
                    "solutionid", "uniquename", "friendlyname", "version", "ismanaged",
                    "publisherid", "installedon", "createdby", "modifiedon", "description"
                ),
                Orders = { new OrderExpression("friendlyname", OrderType.Ascending) }
            };

            // Add filter based on solution type
            if (solutionType.ToLower() != "all")
            {
                bool isManaged = solutionType.ToLower() == "managed";
                solutionQuery.Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("ismanaged", ConditionOperator.Equal, isManaged) }
                };
            }

            // Retrieve all solutions by handling paging
            var allSolutions = new List<Entity>();
            var pageNumber = 1;
            const int pageSize = 5000; // Maximum page size

            do
            {
                solutionQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = pageSize
                };

                var result = await serviceClient.RetrieveMultipleAsync(solutionQuery);
                allSolutions.AddRange(result.Entities);

                if (!result.MoreRecords)
                    break;

                pageNumber++;
            } while (true);

            var solutions = allSolutions.Select(e => new
            {
                SolutionId = e.GetAttributeValue<Guid>("solutionid"),
                UniqueName = e.GetAttributeValue<string>("uniquename"),
                FriendlyName = e.GetAttributeValue<string>("friendlyname"),
                Version = e.GetAttributeValue<string>("version"),
                IsManaged = e.GetAttributeValue<bool>("ismanaged"),
                PublisherId = e.GetAttributeValue<EntityReference>("publisherid")?.Id,
                PublisherName = e.GetAttributeValue<EntityReference>("publisherid")?.Name,
                InstalledOn = e.GetAttributeValue<DateTime?>("installedon"),
                CreatedBy = e.GetAttributeValue<EntityReference>("createdby")?.Name,
                ModifiedOn = e.GetAttributeValue<DateTime?>("modifiedon"),
                Description = e.GetAttributeValue<string>("description")
            }).ToList();

            var resultInfo = new
            {
                SolutionType = solutionType,
                TotalCount = solutions.Count,
                Solutions = solutions
            };

            return JsonSerializer.Serialize(resultInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving solutions: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific solution
    /// </summary>
    /// <param name="solutionName">The unique name of the solution</param>
    /// <returns>JSON string containing detailed solution information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific solution including components and dependencies.")]
    public static async Task<string> ReadSolutionDetails(string solutionName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Get solution details
            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("uniquename", ConditionOperator.Equal, solutionName) }
                }
            };

            var solutionResult = await serviceClient.RetrieveMultipleAsync(solutionQuery);

            if (solutionResult.Entities.Count == 0)
            {
                return $"Solution '{solutionName}' not found.";
            }

            var solution = solutionResult.Entities[0];
            var solutionId = solution.GetAttributeValue<Guid>("solutionid");

            // Get solution components
            var componentQuery = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("solutioncomponentid", "componenttype", "objectid", "rootcomponentbehavior"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId) }
                },
                Orders = { new OrderExpression("componenttype", OrderType.Ascending) }
            };

            var componentResult = await serviceClient.RetrieveMultipleAsync(componentQuery);

            var solutionDetails = new
            {
                SolutionId = solution.GetAttributeValue<Guid>("solutionid"),
                UniqueName = solution.GetAttributeValue<string>("uniquename"),
                FriendlyName = solution.GetAttributeValue<string>("friendlyname"),
                Version = solution.GetAttributeValue<string>("version"),
                IsManaged = solution.GetAttributeValue<bool>("ismanaged"),
                PublisherId = solution.GetAttributeValue<EntityReference>("publisherid")?.Id,
                PublisherName = solution.GetAttributeValue<EntityReference>("publisherid")?.Name,
                InstalledOn = solution.GetAttributeValue<DateTime?>("installedon"),
                CreatedBy = solution.GetAttributeValue<EntityReference>("createdby")?.Name,
                CreatedOn = solution.GetAttributeValue<DateTime?>("createdon"),
                ModifiedBy = solution.GetAttributeValue<EntityReference>("modifiedby")?.Name,
                ModifiedOn = solution.GetAttributeValue<DateTime?>("modifiedon"),
                Description = solution.GetAttributeValue<string>("description"),
                ComponentCount = componentResult.Entities.Count,
                Components = componentResult.Entities.Select(c => new
                {
                    ComponentId = c.GetAttributeValue<Guid>("solutioncomponentid"),
                    ComponentType = c.GetAttributeValue<OptionSetValue>("componenttype")?.Value,
                    ObjectId = c.GetAttributeValue<Guid>("objectid"),
                    RootComponentBehavior = c.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value
                }).ToList()
            };

            return JsonSerializer.Serialize(solutionDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving solution details: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves all solution publishers in the environment
    /// </summary>
    /// <returns>JSON string containing all solution publishers</returns>
    [McpServerTool, Description("Retrieves all solution publishers in the Dataverse environment.")]
    public static async Task<string> ReadPublishers()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var publisherQuery = new QueryExpression("publisher")
            {
                ColumnSet = new ColumnSet(
                    "publisherid", "uniquename", "friendlyname", "customizationprefix",
                    "customizationoptionvalueprefix", "description", "isreadonly"
                ),
                Orders = { new OrderExpression("friendlyname", OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(publisherQuery);

            var publishers = result.Entities.Select(e => new
            {
                PublisherId = e.GetAttributeValue<Guid>("publisherid"),
                UniqueName = e.GetAttributeValue<string>("uniquename"),
                FriendlyName = e.GetAttributeValue<string>("friendlyname"),
                CustomizationPrefix = e.GetAttributeValue<string>("customizationprefix"),
                CustomizationOptionValuePrefix = e.GetAttributeValue<int?>("customizationoptionvalueprefix"),
                Description = e.GetAttributeValue<string>("description"),
                IsReadOnly = e.GetAttributeValue<bool>("isreadonly")
            }).ToList();

            return JsonSerializer.Serialize(publishers, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving publishers: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves components for a specific solution
    /// </summary>
    /// <param name="solutionName">The unique name of the solution</param>
    /// <returns>JSON string containing solution components with detailed information</returns>
    [McpServerTool, Description("Retrieves all components for a specific solution with detailed component type information.")]
    public static async Task<string> ReadSolutionComponents(string solutionName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // First get the solution ID
            var solutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "friendlyname"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("uniquename", ConditionOperator.Equal, solutionName) }
                }
            };

            var solutionResult = await serviceClient.RetrieveMultipleAsync(solutionQuery);

            if (solutionResult.Entities.Count == 0)
            {
                return $"Solution '{solutionName}' not found.";
            }

            var solution = solutionResult.Entities[0];
            var solutionId = solution.GetAttributeValue<Guid>("solutionid");

            // Get solution components
            var componentQuery = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("solutioncomponentid", "componenttype", "objectid", "rootcomponentbehavior", "rootsolutioncomponentid"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId) }
                },
                Orders = { new OrderExpression("componenttype", OrderType.Ascending) }
            };

            var componentResult = await serviceClient.RetrieveMultipleAsync(componentQuery);

            var components = componentResult.Entities.Select(c => new
            {
                ComponentId = c.GetAttributeValue<Guid>("solutioncomponentid"),
                ComponentType = c.GetAttributeValue<OptionSetValue>("componenttype")?.Value,
                ComponentTypeName = GetComponentTypeName(c.GetAttributeValue<OptionSetValue>("componenttype")?.Value),
                ObjectId = c.GetAttributeValue<Guid>("objectid"),
                RootComponentBehavior = c.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")?.Value,
                RootSolutionComponentId = c.GetAttributeValue<Guid?>("rootsolutioncomponentid")
            }).ToList();

            var result = new
            {
                SolutionName = solution.GetAttributeValue<string>("friendlyname"),
                SolutionUniqueName = solutionName,
                ComponentCount = components.Count,
                ComponentsByType = components.GroupBy(c => c.ComponentTypeName)
                    .Select(g => new
                    {
                        ComponentType = g.Key,
                        Count = g.Count(),
                        Components = g.ToList()
                    })
                    .OrderBy(g => g.ComponentType)
                    .ToList()
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving solution components: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a human-readable name for component types
    /// </summary>
    /// <param name="componentType">The component type value</param>
    /// <returns>A descriptive name for the component type</returns>
    private static string GetComponentTypeName(int? componentType)
    {
        return componentType switch
        {
            1 => "Entity",
            2 => "Attribute",
            3 => "Relationship",
            4 => "Attribute Picklist Value",
            5 => "Attribute Lookup Value",
            6 => "View Display String",
            7 => "View",
            8 => "Form",
            9 => "Organization",
            10 => "Saved Query",
            11 => "Workflow",
            12 => "Report",
            13 => "Report Entity",
            14 => "Report Category",
            15 => "Report Visibility",
            16 => "Attachment",
            17 => "Email Template",
            18 => "Contract Template",
            19 => "KB Article Template",
            20 => "Mail Merge Template",
            21 => "Duplicate Rule",
            22 => "Duplicate Rule Condition",
            23 => "Entity Map",
            24 => "Attribute Map",
            25 => "Ribbon Command",
            26 => "Ribbon Context Group",
            27 => "Ribbon Customization",
            28 => "Ribbon Rule",
            29 => "Ribbon Tab To Command Map",
            30 => "Ribbon Diff",
            31 => "Saved Query Visualization",
            32 => "System Form",
            33 => "Web Resource",
            34 => "Site Map",
            35 => "Connection Role",
            36 => "Field Security Profile",
            37 => "Field Permission",
            38 => "Plugin Type",
            39 => "Plugin Assembly",
            44 => "Role",
            45 => "Role Privilege",
            46 => "Display String",
            47 => "Display String Map",
            48 => "Form",
            50 => "Entity Relationship",
            55 => "Global Option Set",
            59 => "Chart",
            60 => "Email Template",
            61 => "Contract Template",
            62 => "KB Article Template",
            63 => "Mail Merge Template",
            64 => "Duplicate Detection Rule",
            65 => "Duplicate Detection Rule Condition",
            66 => "Entity Map",
            67 => "Attribute Map",
            68 => "Ribbon Command",
            69 => "Ribbon Context Group",
            70 => "Ribbon Customization",
            71 => "Ribbon Rule",
            72 => "Ribbon Tab To Command Map",
            73 => "Ribbon Diff",
            _ => $"Unknown ({componentType})"
        };
    }

    #endregion
}
