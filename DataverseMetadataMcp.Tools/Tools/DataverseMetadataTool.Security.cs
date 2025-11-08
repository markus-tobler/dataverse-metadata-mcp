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
    #region Security and Privileges Metadata Methods

    /// <summary>
    /// Retrieves all security roles from Dataverse
    /// </summary>
    /// <returns>JSON string containing all security roles</returns>
    [McpServerTool, Description("Retrieves all security roles from Dataverse.")]
    public static async Task<string> ReadSecurityRoles()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("role")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("roleid", "name", "businessunitid", "iscustomizable", "ismanaged"),
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var securityRoles = result.Entities.Select(e => new
            {
                RoleId = e.GetAttributeValue<Guid>("roleid"),
                Name = e.GetAttributeValue<string>("name"),
                BusinessUnitId = e.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("businessunitid")?.Id,
                BusinessUnitName = e.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("businessunitid")?.Name,
                IsCustomizable = e.GetAttributeValue<Microsoft.Xrm.Sdk.BooleanManagedProperty>("iscustomizable")?.Value,
                IsManaged = e.GetAttributeValue<bool>("ismanaged")
            }).ToList();

            return JsonSerializer.Serialize(securityRoles, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving security roles: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves entity privileges for a specific table
    /// </summary>
    /// <param name="tableName">The logical name of the table</param>
    /// <returns>JSON string containing privileges for the table</returns>
    [McpServerTool, Description("Retrieves entity privileges for a specific table from Dataverse.")]
    public static async Task<string> ReadEntityPrivileges(string tableName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("privilege")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("privilegeid", "name", "accessright", "canbebasic", "canbedeep", "canbeglobal", "canbelocal", "canbeparententityreference"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("name", Microsoft.Xrm.Sdk.Query.ConditionOperator.Like, $"prv%{tableName}")
                    }
                },
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var privileges = result.Entities.Select(e => new
            {
                PrivilegeId = e.GetAttributeValue<Guid>("privilegeid"),
                Name = e.GetAttributeValue<string>("name"),
                AccessRight = e.GetAttributeValue<int>("accessright"),
                AccessRightName = GetAccessRightName(e.GetAttributeValue<int>("accessright")),
                CanBeBasic = e.GetAttributeValue<bool>("canbebasic"),
                CanBeDeep = e.GetAttributeValue<bool>("canbedeep"),
                CanBeGlobal = e.GetAttributeValue<bool>("canbeglobal"),
                CanBeLocal = e.GetAttributeValue<bool>("canbelocal"),
                CanBeParentEntityReference = e.GetAttributeValue<bool>("canbeparententityreference")
            }).ToList();

            return JsonSerializer.Serialize(privileges, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving entity privileges: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves privileges assigned to a specific security role
    /// </summary>
    /// <param name="roleName">The name of the security role</param>
    /// <returns>JSON string containing privileges assigned to the role</returns>
    [McpServerTool, Description("Retrieves privileges assigned to a specific security role from Dataverse.")]
    public static async Task<string> ReadRolePrivileges(string roleName)
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            // First, get the role ID
            var roleQuery = new Microsoft.Xrm.Sdk.Query.QueryExpression("role")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("roleid", "name"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("name", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, roleName)
                    }
                }
            };

            var roleResult = await serviceClient.RetrieveMultipleAsync(roleQuery);
            if (roleResult.Entities.Count == 0)
            {
                return JsonSerializer.Serialize(new { Error = $"Security role '{roleName}' not found" }, new JsonSerializerOptions { WriteIndented = true });
            }

            var roleId = roleResult.Entities[0].GetAttributeValue<Guid>("roleid");

            // Get role privileges
            var privilegeQuery = new Microsoft.Xrm.Sdk.Query.QueryExpression("roleprivileges")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("privilegeid", "privilegedepthmask"),
                Criteria = new Microsoft.Xrm.Sdk.Query.FilterExpression
                {
                    Conditions =
                    {
                        new Microsoft.Xrm.Sdk.Query.ConditionExpression("roleid", Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal, roleId)
                    }
                },
                LinkEntities =
                {
                    new Microsoft.Xrm.Sdk.Query.LinkEntity
                    {
                        LinkFromEntityName = "roleprivileges",
                        LinkFromAttributeName = "privilegeid",
                        LinkToEntityName = "privilege",
                        LinkToAttributeName = "privilegeid",
                        Columns = new Microsoft.Xrm.Sdk.Query.ColumnSet("name", "accessright"),
                        EntityAlias = "priv"
                    }
                }
            };

            var privilegeResult = await serviceClient.RetrieveMultipleAsync(privilegeQuery);

            var rolePrivileges = new
            {
                RoleName = roleName,
                RoleId = roleId,
                Privileges = privilegeResult.Entities.Select(e => new
                {
                    PrivilegeId = e.GetAttributeValue<Guid>("privilegeid"),
                    PrivilegeName = e.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("priv.name")?.Value?.ToString(),
                    AccessRight = e.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("priv.accessright")?.Value,
                    AccessRightName = GetAccessRightName((int)(e.GetAttributeValue<Microsoft.Xrm.Sdk.AliasedValue>("priv.accessright")?.Value ?? 0)),
                    PrivilegeDepthMask = e.GetAttributeValue<int>("privilegedepthmask"),
                    DepthLevel = GetDepthLevelName(e.GetAttributeValue<int>("privilegedepthmask"))
                }).OrderBy(p => p.PrivilegeName).ToList()
            };

            return JsonSerializer.Serialize(rolePrivileges, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving role privileges: {ex.Message}";
        }
    }

    /// <summary>
    /// Retrieves field-level security profiles from Dataverse
    /// </summary>
    /// <returns>JSON string containing field security profiles</returns>
    [McpServerTool, Description("Retrieves field-level security profiles from Dataverse.")]
    public static async Task<string> ReadFieldSecurityProfiles()
    {
        try
        {
            var serviceClient = ConfigurationHelper.GetServiceClient();

            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("fieldsecurityprofile")
            {
                ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("fieldsecurityprofileid", "name", "description", "ismanaged"),
                Orders = { new Microsoft.Xrm.Sdk.Query.OrderExpression("name", Microsoft.Xrm.Sdk.Query.OrderType.Ascending) }
            };

            var result = await serviceClient.RetrieveMultipleAsync(query);

            var fieldSecurityProfiles = result.Entities.Select(e => new
            {
                FieldSecurityProfileId = e.GetAttributeValue<Guid>("fieldsecurityprofileid"),
                Name = e.GetAttributeValue<string>("name"),
                Description = e.GetAttributeValue<string>("description"),
                IsManaged = e.GetAttributeValue<bool>("ismanaged")
            }).ToList();

            return JsonSerializer.Serialize(fieldSecurityProfiles, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return $"Error retrieving field security profiles: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets a human-readable name for access right values
    /// </summary>
    /// <param name="accessRight">The access right value</param>
    /// <returns>A descriptive string of the access right</returns>
    private static string GetAccessRightName(int accessRight)
    {
        return accessRight switch
        {
            1 => "Read",
            2 => "Write",
            4 => "Append",
            16 => "AppendTo",
            32 => "Create",
            65536 => "Delete",
            262144 => "Share",
            524288 => "Assign",
            _ => $"Unknown ({accessRight})"
        };
    }

    /// <summary>
    /// Gets a human-readable name for privilege depth mask values
    /// </summary>
    /// <param name="depthMask">The privilege depth mask value</param>
    /// <returns>A descriptive string of the depth level</returns>
    private static string GetDepthLevelName(int depthMask)
    {
        return depthMask switch
        {
            1 => "Basic (User)",
            2 => "Local (Business Unit)",
            4 => "Deep (Parent: Child Business Units)",
            8 => "Global (Organization)",
            _ => $"Unknown ({depthMask})"
        };
    }

    #endregion
}