using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;
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
    #region Picklist/Choice Metadata Methods

    /// <summary>
    /// Retrieves all global option sets (choices) from Dataverse
    /// </summary>
    /// <returns>JSON string containing all global option sets</returns>
    [McpServerTool, Description("Retrieves all global option sets (choices) from Dataverse.")]
    public static async Task<string> ReadGlobalOptionSets()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveAllOptionSetsRequest();
            var response = (RetrieveAllOptionSetsResponse)await serviceClient.ExecuteAsync(request);

            var optionSets = response.OptionSetMetadata
                .Where(os => os.IsGlobal == true)
                .Select(os => new
                {
                    Name = os.Name,
                    DisplayName = os.DisplayName?.UserLocalizedLabel?.Label ?? os.Name,
                    Description = os.Description?.UserLocalizedLabel?.Label,
                    IsCustomOptionSet = os.IsCustomOptionSet,
                    IsGlobal = os.IsGlobal,
                    OptionSetType = os.OptionSetType?.ToString(),
                    IsManaged = os.IsManaged,
                    OptionCount = os is OptionSetMetadata optionSetMeta ? optionSetMeta.Options?.Count : 0
                })
                .OrderBy(os => os.DisplayName)
                .ToList();

            return JsonSerializer.Serialize(optionSets, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving global option sets: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific global option set
    /// </summary>
    /// <param name="optionSetName">The name of the global option set</param>
    /// <returns>JSON string containing detailed option set information</returns>
    [McpServerTool, Description("Retrieves detailed information about a specific global option set from Dataverse.")]
    public static async Task<string> ReadGlobalOptionSet(string optionSetName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var request = new RetrieveOptionSetRequest
            {
                Name = optionSetName,
                RetrieveAsIfPublished = true
            };

            var response = (RetrieveOptionSetResponse)await serviceClient.ExecuteAsync(request);
            var optionSetMetadata = response.OptionSetMetadata;

            object optionSetDetails;

            if (optionSetMetadata is OptionSetMetadata standardOptionSet)
            {
                optionSetDetails = new
                {
                    Name = standardOptionSet.Name,
                    DisplayName = standardOptionSet.DisplayName?.UserLocalizedLabel?.Label ?? standardOptionSet.Name,
                    Description = standardOptionSet.Description?.UserLocalizedLabel?.Label,
                    IsCustomOptionSet = standardOptionSet.IsCustomOptionSet,
                    IsGlobal = standardOptionSet.IsGlobal,
                    IsManaged = standardOptionSet.IsManaged,
                    OptionSetType = standardOptionSet.OptionSetType?.ToString(),
                    Options = standardOptionSet.Options?.Select(o => new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        IsManaged = o.IsManaged,
                        ExternalValue = o.ExternalValue
                    }).ToList()
                };
            }
            else if (optionSetMetadata is BooleanOptionSetMetadata booleanOptionSet)
            {
                optionSetDetails = new
                {
                    Name = booleanOptionSet.Name,
                    DisplayName = booleanOptionSet.DisplayName?.UserLocalizedLabel?.Label ?? booleanOptionSet.Name,
                    Description = booleanOptionSet.Description?.UserLocalizedLabel?.Label,
                    IsCustomOptionSet = booleanOptionSet.IsCustomOptionSet,
                    IsGlobal = booleanOptionSet.IsGlobal,
                    IsManaged = booleanOptionSet.IsManaged,
                    OptionSetType = booleanOptionSet.OptionSetType?.ToString(),
                    TrueOption = new
                    {
                        Value = booleanOptionSet.TrueOption?.Value,
                        Label = booleanOptionSet.TrueOption?.Label?.UserLocalizedLabel?.Label
                    },
                    FalseOption = new
                    {
                        Value = booleanOptionSet.FalseOption?.Value,
                        Label = booleanOptionSet.FalseOption?.Label?.UserLocalizedLabel?.Label
                    }
                };
            }
            else
            {
                optionSetDetails = new
                {
                    Name = optionSetMetadata.Name,
                    DisplayName = optionSetMetadata.DisplayName?.UserLocalizedLabel?.Label ?? optionSetMetadata.Name,
                    Description = optionSetMetadata.Description?.UserLocalizedLabel?.Label,
                    IsCustomOptionSet = optionSetMetadata.IsCustomOptionSet,
                    IsGlobal = optionSetMetadata.IsGlobal,
                    IsManaged = optionSetMetadata.IsManaged,
                    OptionSetType = optionSetMetadata.OptionSetType?.ToString()
                };
            }

            return JsonSerializer.Serialize(optionSetDetails, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving global option set: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves local option sets for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing local option sets for the table</returns>
    [McpServerTool, Description("Retrieves local option sets (choices) for a specific table from Dataverse.")]
    public static async Task<string> ReadLocalOptionSets(string tableName)
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
            var entity = response.EntityMetadata; var localOptionSets = entity.Attributes
                .Where(a => a is PicklistAttributeMetadata || a is StatusAttributeMetadata || a is StateAttributeMetadata)
                .Select(a =>
                {
                    if (a is PicklistAttributeMetadata picklistAttr && picklistAttr.OptionSet?.IsGlobal != true)
                    {
                        return (object)new
                        {
                            AttributeName = a.LogicalName,
                            DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                            AttributeType = "Picklist",
                            Options = picklistAttr.OptionSet?.Options?.Select(o => new
                            {
                                Value = o.Value,
                                Label = o.Label?.UserLocalizedLabel?.Label,
                                Description = o.Description?.UserLocalizedLabel?.Label,
                                Color = o.Color
                            }).ToList()
                        };
                    }
                    else if (a is StatusAttributeMetadata statusAttr)
                    {
                        return (object)new
                        {
                            AttributeName = a.LogicalName,
                            DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                            AttributeType = "Status",
                            Options = statusAttr.OptionSet?.Options?.Select(o => new
                            {
                                Value = o.Value,
                                Label = o.Label?.UserLocalizedLabel?.Label,
                                Description = o.Description?.UserLocalizedLabel?.Label,
                                Color = o.Color,
                                State = (o as StatusOptionMetadata)?.State
                            }).ToList()
                        };
                    }
                    else if (a is StateAttributeMetadata stateAttr)
                    {
                        return (object)new
                        {
                            AttributeName = a.LogicalName,
                            DisplayName = a.DisplayName?.UserLocalizedLabel?.Label ?? a.LogicalName,
                            AttributeType = "State",
                            Options = stateAttr.OptionSet?.Options?.Select(o => new
                            {
                                Value = o.Value,
                                Label = o.Label?.UserLocalizedLabel?.Label,
                                Description = o.Description?.UserLocalizedLabel?.Label,
                                Color = o.Color
                            }).ToList()
                        };
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToList();

            return JsonSerializer.Serialize(localOptionSets, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving local option sets: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves options for a specific picklist attribute
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="attributeName">The logical name of the picklist attribute</param>
    /// <returns>JSON string containing picklist options</returns>
    [McpServerTool, Description("Retrieves options for a specific picklist attribute from Dataverse.")]
    public static async Task<string> ReadPicklistOptions(string tableName, string attributeName)
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
            var attributeMetadata = response.AttributeMetadata;

            object result = new { Error = "Attribute is not a picklist type" };

            if (attributeMetadata is PicklistAttributeMetadata picklistAttr)
            {
                result = new
                {
                    AttributeName = picklistAttr.LogicalName,
                    DisplayName = picklistAttr.DisplayName?.UserLocalizedLabel?.Label ?? picklistAttr.LogicalName,
                    AttributeType = "Picklist",
                    IsGlobal = picklistAttr.OptionSet?.IsGlobal == true,
                    GlobalOptionSetName = picklistAttr.OptionSet?.IsGlobal == true ? picklistAttr.OptionSet.Name : null,
                    DefaultFormValue = picklistAttr.DefaultFormValue,
                    Options = picklistAttr.OptionSet?.Options?.Select(o => new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        IsManaged = o.IsManaged,
                        ExternalValue = o.ExternalValue
                    }).OrderBy(o => o.Value).ToList()
                };
            }
            else if (attributeMetadata is MultiSelectPicklistAttributeMetadata multiSelectAttr)
            {
                result = new
                {
                    AttributeName = multiSelectAttr.LogicalName,
                    DisplayName = multiSelectAttr.DisplayName?.UserLocalizedLabel?.Label ?? multiSelectAttr.LogicalName,
                    AttributeType = "MultiSelectPicklist",
                    IsGlobal = multiSelectAttr.OptionSet?.IsGlobal == true,
                    GlobalOptionSetName = multiSelectAttr.OptionSet?.IsGlobal == true ? multiSelectAttr.OptionSet.Name : null,
                    DefaultFormValue = multiSelectAttr.DefaultFormValue,
                    Options = multiSelectAttr.OptionSet?.Options?.Select(o => new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        IsManaged = o.IsManaged,
                        ExternalValue = o.ExternalValue
                    }).OrderBy(o => o.Value).ToList()
                };
            }
            else if (attributeMetadata is StatusAttributeMetadata statusAttr)
            {
                result = new
                {
                    AttributeName = statusAttr.LogicalName,
                    DisplayName = statusAttr.DisplayName?.UserLocalizedLabel?.Label ?? statusAttr.LogicalName,
                    AttributeType = "Status",
                    DefaultFormValue = statusAttr.DefaultFormValue,
                    Options = statusAttr.OptionSet?.Options?.Select(o => new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        State = (o as StatusOptionMetadata)?.State,
                        IsManaged = o.IsManaged
                    }).OrderBy(o => o.Value).ToList()
                };
            }
            else if (attributeMetadata is StateAttributeMetadata stateAttr)
            {
                result = new
                {
                    AttributeName = stateAttr.LogicalName,
                    DisplayName = stateAttr.DisplayName?.UserLocalizedLabel?.Label ?? stateAttr.LogicalName,
                    AttributeType = "State",
                    DefaultFormValue = stateAttr.DefaultFormValue,
                    Options = stateAttr.OptionSet?.Options?.Select(o => new
                    {
                        Value = o.Value,
                        Label = o.Label?.UserLocalizedLabel?.Label,
                        Description = o.Description?.UserLocalizedLabel?.Label,
                        Color = o.Color,
                        IsManaged = o.IsManaged
                    }).OrderBy(o => o.Value).ToList()
                };
            }

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving picklist options: {ex.Message}";
        }
    }    /// <summary>
         /// Creates a new global choice (option set) in Dataverse
         /// </summary>
         /// <param name="choiceName">The logical name for the new global choice (must include publisher prefix, e.g., 'new_choicename')</param>
         /// <param name="displayName">The display name for the choice</param>
         /// <param name="description">Description of the choice's purpose</param>
         /// <param name="choiceOptions">JSON array of choice options in format: [{"value": 1, "label": "Option 1"}, {"value": 2, "label": "Option 2"}]</param>
         /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Creates a new global choice (option set) in Dataverse with custom options and comprehensive validation. Choice name must include publisher prefix (e.g., 'new_choicename').")]
    public static async Task<string> CreateGlobalChoice(
        string choiceName,
        string displayName,
        string description,
        string choiceOptions,
        string? publisherName = null)
    {
        try
        {
            // Validate inputs
            var validationResult = ValidateGlobalChoiceInputs(choiceName, displayName, description, choiceOptions);
            if (!validationResult.IsValid)
                return CreateGlobalChoiceErrorResponse("Input Validation Failed", validationResult.Errors);

            if (string.IsNullOrWhiteSpace(publisherName))
                return CreateGlobalChoiceErrorResponse("Input Validation Failed", new[] { "Publisher name is required." });

            // Parse and validate choice options (now just labels)
            List<string> labels;
            try
            {
                labels = JsonSerializer.Deserialize<List<string>>(choiceOptions) ?? new List<string>();
            }
            catch (JsonException)
            {
                return CreateGlobalChoiceErrorResponse(
                    "Invalid Choice Options",
                    new[] {
                        "Choice options must be a valid JSON array of labels.",
                        "Example: [\"Option 1\", \"Option 2\"]"
                    }
                );
            }

            if (labels.Count == 0)
                return CreateGlobalChoiceErrorResponse("No Choice Options", new[] { "At least one choice option label is required." });

            if (labels.Any(l => string.IsNullOrWhiteSpace(l)))
                return CreateGlobalChoiceErrorResponse("Invalid Choice Options", new[] { "All choice options must have a non-empty label." });

            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Retrieve publisher option value prefix
            var publisherPrefixInt = GetPublisherOptionValuePrefix(serviceClient, publisherName);
            if (publisherPrefixInt == null)
                return CreateGlobalChoiceErrorResponse("Publisher Prefix Not Found", new[] { $"Could not find publisher option value prefix for '{publisherName}'." });

            // Generate values using publisher prefix
            var prefixBase = publisherPrefixInt.Value * 10000;
            var optionValues = Enumerable.Range(prefixBase, labels.Count).ToList();

            // Create option metadata
            var optionMetadata = labels.Select((label, idx) => new OptionMetadata(new Label(label, 1033), optionValues[idx])).ToList();

            var logicalName = ValidateAndCleanChoiceName(choiceName);
            var optionSetMetadata = new OptionSetMetadata
            {
                Name = logicalName,
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033),
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist,
                Options = { }
            };

            foreach (var option in optionMetadata)
            {
                optionSetMetadata.Options.Add(option);
            }

            var request = new CreateOptionSetRequest
            {
                OptionSet = optionSetMetadata
            };

            var response = (CreateOptionSetResponse)await serviceClient.ExecuteAsync(request);

            var optionsResult = labels.Select((label, idx) => new { Value = optionValues[idx], Label = label }).ToList();

            return CreateGlobalChoiceSuccessResponse("Global Choice Created Successfully", new
            {
                LogicalName = logicalName,
                DisplayName = displayName,
                Description = description,
                Options = optionsResult,
                OptionSetId = response.OptionSetId,
                PublisherOptionValuePrefix = publisherPrefixInt,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return CreateGlobalChoiceErrorResponse("Error Creating Global Choice", new[] { ex.Message });
        }
    }

    #endregion

    #region Status Code (Status Reason) Management Methods

    /// <summary>
    /// Adds a new status (statuscode) option to a table.
    /// </summary>
    /// <param name="tableName">The logical name of the table.</param>
    /// <param name="statusLabel">The display label for the new status option.</param>
    /// <param name="stateCode">The state code this status option belongs to (0 = Active, 1 = Inactive).</param>
    /// <param name="publisherName">The friendly name of the publisher. Used to determine the numerical option value prefix for the new status option.</param>
    /// <param name="description">Optional description for the status option.</param>
    /// <param name="languageCode">Language code for labels (default: 1033 for English).</param>
    /// <param name="solutionUniqueName">Optional solution unique name to add the status to.</param>
    /// <returns>JSON string containing the result of the operation, including the assigned option value using the publisher's prefix.</returns>
    [McpServerTool, Description("Adds a new status (statuscode/status reason) option to a table for the specified state code.")]
    public static async Task<string> AddStatusOption(
        string tableName,
        string statusLabel,
        int stateCode,
        string? publisherName = null,
        string? description = null,
        int languageCode = 1033,
        string? solutionUniqueName = null)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Validation
            if (string.IsNullOrWhiteSpace(tableName))
                return CreateStatusResponse(false, "Table name is required.", null);

            if (string.IsNullOrWhiteSpace(statusLabel))
                return CreateStatusResponse(false, "Status label is required.", null);

            if (stateCode < 0)
                return CreateStatusResponse(false, "State code must be a valid non-negative integer.", null);

            if (string.IsNullOrWhiteSpace(publisherName))
                return CreateStatusResponse(false, "Publisher name is required.", null);

            // Retrieve publisher option value prefix
            var publisherPrefixInt = GetPublisherOptionValuePrefix(serviceClient, publisherName);
            if (publisherPrefixInt == null)
                return CreateStatusResponse(false, $"Could not find publisher option value prefix for '{publisherName}'.", null);

            // Get all existing statuscode values for the table
            var existingValues = GetExistingStatusOptionValues(serviceClient, tableName);
            // Find the highest value with the publisher prefix
            var prefixBase = publisherPrefixInt.Value * 10000; // e.g. 72700 -> 727000000
            var maxValue = existingValues.Where(v => v >= prefixBase && v < prefixBase + 10000).DefaultIfEmpty(prefixBase - 1).Max();
            var nextValue = maxValue + 1;

            var request = new InsertStatusValueRequest
            {
                EntityLogicalName = tableName.ToLowerInvariant(),
                AttributeLogicalName = "statuscode",
                Label = new Label(statusLabel, languageCode),
                StateCode = stateCode,
                SolutionUniqueName = solutionUniqueName,
                Value = nextValue
            };

            if (!string.IsNullOrWhiteSpace(description))
            {
                request.Description = new Label(description, languageCode);
            }

            var response = (InsertStatusValueResponse)await serviceClient.ExecuteAsync(request);

            var result = new
            {
                TableName = tableName,
                NewStatusValue = response.NewOptionValue,
                StatusLabel = statusLabel,
                StateCode = stateCode,
                Description = description,
                LanguageCode = languageCode,
                SolutionUniqueName = solutionUniqueName,
                PublisherOptionValuePrefix = publisherPrefixInt,
                AssignedValue = nextValue
            };

            return CreateStatusResponse(true, $"Status option '{statusLabel}' added successfully with value {response.NewOptionValue}.", result);
        }
        catch (Exception ex)
        {
            return CreateStatusResponse(false, $"Error adding status option: {ex.Message}", new { ErrorDetails = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Retrieves the publisher's customizationoptionvalueprefix from Dataverse
    /// </summary>
    private static int? GetPublisherOptionValuePrefix(IOrganizationService serviceClient, string publisherName)
    {
        var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("publisher")
        {
            ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("customizationoptionvalueprefix"),
            Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
            {
                FilterOperator = Microsoft.Xrm.Sdk.Query.LogicalOperator.Or,
                Conditions =
                {
                    new Microsoft.Xrm.Sdk.Query.ConditionExpression("friendlyname", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, publisherName),
                    new Microsoft.Xrm.Sdk.Query.ConditionExpression("uniquename", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, publisherName)
                }
            }
        };
        var publishers = serviceClient.RetrieveMultiple(query);
        var publisher = publishers.Entities.FirstOrDefault();
        if (publisher != null && publisher.Attributes.Contains("customizationoptionvalueprefix"))
        {
            return (int)publisher["customizationoptionvalueprefix"];
        }
        return null;
    }

    /// <summary>
    /// Gets all existing statuscode option values for a table
    /// </summary>
    private static List<int> GetExistingStatusOptionValues(IOrganizationService serviceClient, string tableName)
    {
        var values = new List<int>();
        var request = new RetrieveEntityRequest
        {
            LogicalName = tableName.ToLowerInvariant(),
            EntityFilters = EntityFilters.Attributes,
            RetrieveAsIfPublished = true
        };
        var response = (RetrieveEntityResponse)serviceClient.Execute(request);
        var entity = response.EntityMetadata;
        var statusAttr = entity.Attributes?.FirstOrDefault(a => a.LogicalName == "statuscode") as StatusAttributeMetadata;
        if (statusAttr?.OptionSet?.Options != null)
        {
            foreach (var option in statusAttr.OptionSet.Options)
            {
                if (option.Value.HasValue)
                    values.Add(option.Value.Value);
            }
        }
        return values;
    }

    /// <summary>
    /// Updates an existing status (statuscode) option in a table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="statusValue">The value of the status option to update</param>
    /// <param name="newLabel">The new display label for the status option</param>
    /// <param name="newDescription">Optional new description for the status option</param>
    /// <param name="languageCode">Language code for labels (default: 1033 for English)</param>
    /// <param name="solutionUniqueName">Optional solution unique name</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Updates an existing status (statuscode/status reason) option label and description in a table.")]
    public static async Task<string> UpdateStatusOption(
        string tableName,
        int statusValue,
        string newLabel,
        string? newDescription = null,
        int languageCode = 1033,
        string? solutionUniqueName = null)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Validation
            if (string.IsNullOrWhiteSpace(tableName))
                return CreateStatusResponse(false, "Table name is required.", null);

            if (string.IsNullOrWhiteSpace(newLabel))
                return CreateStatusResponse(false, "New label is required.", null);

            var request = new UpdateOptionValueRequest
            {
                EntityLogicalName = tableName.ToLowerInvariant(),
                AttributeLogicalName = "statuscode",
                Value = statusValue,
                Label = new Label(newLabel, languageCode),
                SolutionUniqueName = solutionUniqueName,
                MergeLabels = true
            };

            if (!string.IsNullOrWhiteSpace(newDescription))
            {
                request.Description = new Label(newDescription, languageCode);
            }

            await serviceClient.ExecuteAsync(request);

            var result = new
            {
                TableName = tableName,
                StatusValue = statusValue,
                NewLabel = newLabel,
                NewDescription = newDescription,
                LanguageCode = languageCode,
                SolutionUniqueName = solutionUniqueName
            };

            return CreateStatusResponse(true, $"Status option with value {statusValue} updated successfully.", result);
        }
        catch (Exception ex)
        {
            return CreateStatusResponse(false, $"Error updating status option: {ex.Message}", new { ErrorDetails = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Deletes a status (statuscode) option from a table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="statusValue">The value of the status option to delete</param>
    /// <param name="solutionUniqueName">Optional solution unique name</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Deletes a status (statuscode/status reason) option from a table. Cannot delete system-required status options.")]
    public static async Task<string> DeleteStatusOption(
        string tableName,
        int statusValue,
        string? solutionUniqueName = null)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Validation
            if (string.IsNullOrWhiteSpace(tableName))
                return CreateStatusResponse(false, "Table name is required.", null);

            var request = new DeleteOptionValueRequest
            {
                EntityLogicalName = tableName.ToLowerInvariant(),
                AttributeLogicalName = "statuscode",
                Value = statusValue,
                SolutionUniqueName = solutionUniqueName
            };

            await serviceClient.ExecuteAsync(request);

            var result = new
            {
                TableName = tableName,
                DeletedStatusValue = statusValue,
                SolutionUniqueName = solutionUniqueName
            };

            return CreateStatusResponse(true, $"Status option with value {statusValue} deleted successfully.", result);
        }
        catch (Exception ex)
        {
            return CreateStatusResponse(false, $"Error deleting status option: {ex.Message}", new { ErrorDetails = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Updates a state (statecode) option label in a table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <param name="stateValue">The value of the state option to update</param>
    /// <param name="newLabel">The new display label for the state option</param>
    /// <param name="defaultStatusCode">Optional default status code for this state</param>
    /// <param name="languageCode">Language code for labels (default: 1033 for English)</param>
    /// <returns>JSON string containing the result of the operation</returns>
    [McpServerTool, Description("Updates an existing state (statecode) option label. Note: State values cannot be added or deleted for standard tables.")]
    public static async Task<string> UpdateStateOption(
        string tableName,
        int stateValue,
        string newLabel,
        int? defaultStatusCode = null,
        int languageCode = 1033)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // Validation
            if (string.IsNullOrWhiteSpace(tableName))
                return CreateStatusResponse(false, "Table name is required.", null);

            if (string.IsNullOrWhiteSpace(newLabel))
                return CreateStatusResponse(false, "New label is required.", null);

            var request = new UpdateStateValueRequest
            {
                EntityLogicalName = tableName.ToLowerInvariant(),
                AttributeLogicalName = "statecode",
                Value = stateValue,
                Label = new Label(newLabel, languageCode),
                MergeLabels = true
            };

            if (defaultStatusCode.HasValue)
            {
                request.DefaultStatusCode = defaultStatusCode.Value;
            }

            await serviceClient.ExecuteAsync(request);

            var result = new
            {
                TableName = tableName,
                StateValue = stateValue,
                NewLabel = newLabel,
                DefaultStatusCode = defaultStatusCode,
                LanguageCode = languageCode
            };

            return CreateStatusResponse(true, $"State option with value {stateValue} updated successfully.", result);
        }
        catch (Exception ex)
        {
            return CreateStatusResponse(false, $"Error updating state option: {ex.Message}", new { ErrorDetails = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Retrieves state and status transition information for a table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing state/status transition information</returns>
    [McpServerTool, Description("Retrieves state and status transition information for a table, including allowed transitions and enforcement settings.")]
    public static async Task<string> ReadStateStatusTransitions(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            if (string.IsNullOrWhiteSpace(tableName))
                return CreateStatusResponse(false, "Table name is required.", null);

            // Get entity metadata with state and status information
            var entityRequest = new RetrieveEntityRequest
            {
                LogicalName = tableName.ToLowerInvariant(),
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true
            };

            var entityResponse = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(entityRequest);
            var entity = entityResponse.EntityMetadata;

            var stateAttribute = entity.Attributes?.FirstOrDefault(a => a.LogicalName == "statecode") as StateAttributeMetadata;
            var statusAttribute = entity.Attributes?.FirstOrDefault(a => a.LogicalName == "statuscode") as StatusAttributeMetadata;

            if (stateAttribute == null || statusAttribute == null)
            {
                return CreateStatusResponse(false, "This table does not have standard state/status attributes.", null);
            }

            var stateOptions = stateAttribute.OptionSet?.Options?.Cast<StateOptionMetadata>().Select(s => new
            {
                Value = s.Value,
                Label = s.Label?.UserLocalizedLabel?.Label,
                DefaultStatus = s.DefaultStatus,
                InvariantName = s.InvariantName
            }).ToList();

            var statusOptions = statusAttribute.OptionSet?.Options?.Cast<StatusOptionMetadata>().Select(s => new
            {
                Value = s.Value,
                Label = s.Label?.UserLocalizedLabel?.Label,
                State = s.State,
                TransitionData = s.TransitionData,
                Color = s.Color
            }).ToList();

            var result = new
            {
                TableName = tableName,
                EnforceStateTransitions = entity.EnforceStateTransitions,
                StateOptions = stateOptions,
                StatusOptions = statusOptions,
                TransitionInfo = statusOptions?.Where(s => !string.IsNullOrEmpty(s.TransitionData)).Select(s => new
                {
                    StatusValue = s.Value,
                    StatusLabel = s.Label,
                    AllowedTransitions = ParseTransitionData(s.TransitionData)
                }).ToList()
            };

            return CreateStatusResponse(true, "State and status transition information retrieved successfully.", result);
        }
        catch (Exception ex)
        {
            return CreateStatusResponse(false, $"Error retrieving state/status transitions: {ex.Message}", new { ErrorDetails = ex.InnerException?.Message });
        }
    }

    #endregion

    #region Status Code Helper Methods

    /// <summary>
    /// Creates a standardized response for status/state operations
    /// </summary>
    private static string CreateStatusResponse(bool success, string message, object? data)
    {
        var response = new
        {
            Success = success,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Parses transition data XML to extract allowed transitions
    /// </summary>
    private static List<object> ParseTransitionData(string? transitionData)
    {
        var transitions = new List<object>();

        if (string.IsNullOrEmpty(transitionData))
            return transitions;

        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(transitionData);
            var allowedTransitions = doc.Descendants().Where(e => e.Name.LocalName == "allowedtransition");

            foreach (var transition in allowedTransitions)
            {
                transitions.Add(new
                {
                    FromStatusId = transition.Attribute("sourcestatusid")?.Value,
                    ToStatusId = transition.Attribute("tostatusid")?.Value
                });
            }
        }
        catch
        {
            // If XML parsing fails, return empty list
        }

        return transitions;
    }

    #endregion

    #region Global Choice Helper Methods    /// <summary>
    /// Validates inputs for global choice creation
    /// </summary>
    private static GlobalChoiceValidationResult ValidateGlobalChoiceInputs(string choiceName, string displayName, string description, string choiceOptions)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(choiceName))
            errors.Add("Choice name is required.");

        if (string.IsNullOrWhiteSpace(displayName))
            errors.Add("Display name is required.");

        if (string.IsNullOrWhiteSpace(description))
            errors.Add("Description is required.");

        if (string.IsNullOrWhiteSpace(choiceOptions))
            errors.Add("Choice options are required.");

        // Validate choice name format
        if (!string.IsNullOrWhiteSpace(choiceName))
        {
            if (choiceName.Length > 100)
                errors.Add("Choice name cannot exceed 100 characters.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(choiceName, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
                errors.Add("Choice name must start with a letter and contain only letters, numbers, and underscores.");

            // Check that it has a valid publisher prefix
            if (!System.Text.RegularExpressions.Regex.IsMatch(choiceName, @"^(new_|cr[a-f0-9]{2,5}_|[a-z]{2,8}_)[a-z][a-z0-9_]*$"))
                errors.Add("Choice name must include a valid publisher prefix (e.g., 'new_', 'cr123_', or custom publisher prefix).");

            // Check for reserved prefixes that might conflict with system choices
            var reservedPrefixes = new[] { "ms_", "microsoft_", "dynamics_", "system_" };
            if (reservedPrefixes.Any(prefix => choiceName.ToLower().StartsWith(prefix)))
                errors.Add($"Choice name cannot start with reserved prefixes: {string.Join(", ", reservedPrefixes)}");
        }

        return new GlobalChoiceValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    /// <summary>
    /// Validates and cleans the choice name (ensures proper format but doesn't add prefixes)
    /// </summary>
    private static string ValidateAndCleanChoiceName(string choiceName)
    {
        if (string.IsNullOrWhiteSpace(choiceName))
            return choiceName;

        // Convert to lowercase and clean up
        var result = choiceName.ToLower();

        // Replace spaces and special characters with underscores (but preserve the prefix structure)
        var parts = result.Split('_', 2);
        if (parts.Length == 2)
        {
            // Keep the prefix as-is, clean the rest
            var prefix = parts[0];
            var name = parts[1];
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-z0-9_]", "_");
            name = System.Text.RegularExpressions.Regex.Replace(name, @"_{2,}", "_");
            name = name.Trim('_');
            result = prefix + "_" + name;
        }
        else
        {
            // No underscore found, clean the whole thing
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^a-z0-9_]", "_");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"_{2,}", "_");
            result = result.Trim('_');
        }

        return result;
    }

    /// <summary>
    /// Creates a success response JSON for global choice operations
    /// </summary>
    private static string CreateGlobalChoiceSuccessResponse(string message, object data)
    {
        var response = new
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Creates an error response JSON for global choice operations
    /// </summary>
    private static string CreateGlobalChoiceErrorResponse(string message, IEnumerable<string> errors)
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

    /// <summary>
    /// Validation result structure for global choices
    /// </summary>
    private class GlobalChoiceValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a choice option for global choices
    /// </summary>
    private class GlobalChoiceOption
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    #endregion
}