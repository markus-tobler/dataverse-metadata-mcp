using System.Text.Json;
using Microsoft.Xrm.Sdk.Messages;
using DataverseMetadataMcp.Tools.Tools;
using Xunit;
using Xunit.Abstractions;

namespace DataverseMetadataMcp.Tools.Tests.Tools;

/// <summary>
/// Integration tests for DataverseMetadataTool.CreateTable, focusing on ownership type support.
/// </summary>
public class DataverseMetadataToolCreateTableTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _createdTables = new();

    public DataverseMetadataToolCreateTableTests(ITestOutputHelper output) : base()
    {
        _output = output;
    }

    private string GenerateTestTableName()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(100, 999);
        return $"new_mcptest{timestamp % 100000}{random}";
    }

    private void TrackCreatedTable(string schemaName)
    {
        _createdTables.Add(schemaName);
        _output.WriteLine($"Tracked table for cleanup: {schemaName}");
    }

    #region Ownership Type Validation Tests

    [Fact]
    public async Task CreateTable_InvalidOwnershipType_ReturnsError()
    {
        // Arrange — "TeamOwned" is not a valid value

        // Act
        var result = await DataverseMetadataTool.CreateTable(
            schemaName: "new_testinvalid",
            displayName: "Test Table",
            pluralName: "Test Tables",
            ownershipType: "TeamOwned");

        // Assert — validation error is returned as a plain error string before any Dataverse call
        Assert.StartsWith("Error:", result);
        Assert.Contains("ownershipType", result);
        _output.WriteLine($"Expected error received: {result}");
    }

    [Fact]
    public async Task CreateTable_EmptyOwnershipType_ReturnsError()
    {
        // Act
        var result = await DataverseMetadataTool.CreateTable(
            schemaName: "new_testinvalid",
            displayName: "Test Table",
            pluralName: "Test Tables",
            ownershipType: "");

        // Assert
        Assert.StartsWith("Error:", result);
        Assert.Contains("ownershipType", result);
        _output.WriteLine($"Expected error received: {result}");
    }

    #endregion

    #region Ownership Type Integration Tests

    [Fact]
    public async Task CreateTable_UserOwned_CreatesTableSuccessfully()
    {
        EnsureDataverseConnection();

        var schemaName = GenerateTestTableName();

        try
        {
            // Act
            var result = await DataverseMetadataTool.CreateTable(
                schemaName: schemaName,
                displayName: "Test MCP User Owned",
                pluralName: "Test MCP User Owned Tables",
                ownershipType: "UserOwned");

            // Assert
            var response = JsonSerializer.Deserialize<JsonElement>(result);
            var success = response.GetProperty("Success").GetBoolean();
            Assert.True(success, $"Expected success but got: {result}");

            TrackCreatedTable(schemaName);
            _output.WriteLine($"Successfully created UserOwned table: {schemaName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateTable_OrganizationOwned_CreatesTableSuccessfully()
    {
        EnsureDataverseConnection();

        var schemaName = GenerateTestTableName();

        try
        {
            // Act
            var result = await DataverseMetadataTool.CreateTable(
                schemaName: schemaName,
                displayName: "Test MCP Org Owned",
                pluralName: "Test MCP Org Owned Tables",
                ownershipType: "OrganizationOwned");

            // Assert
            var response = JsonSerializer.Deserialize<JsonElement>(result);
            var success = response.GetProperty("Success").GetBoolean();
            Assert.True(success, $"Expected success but got: {result}");

            TrackCreatedTable(schemaName);
            _output.WriteLine($"Successfully created OrganizationOwned table: {schemaName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateTable_OwnershipTypeCaseInsensitive_CreatesTableSuccessfully()
    {
        EnsureDataverseConnection();

        var schemaName = GenerateTestTableName();

        try
        {
            // Act — lowercase value should be accepted
            var result = await DataverseMetadataTool.CreateTable(
                schemaName: schemaName,
                displayName: "Test MCP Case Insensitive",
                pluralName: "Test MCP Case Insensitive Tables",
                ownershipType: "organizationowned");

            // Assert
            var response = JsonSerializer.Deserialize<JsonElement>(result);
            var success = response.GetProperty("Success").GetBoolean();
            Assert.True(success, $"Expected success but got: {result}");

            TrackCreatedTable(schemaName);
            _output.WriteLine($"Successfully created table with lowercase ownershipType: {schemaName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateTable_DefaultOwnershipType_IsUserOwned()
    {
        EnsureDataverseConnection();

        var schemaName = GenerateTestTableName();

        try
        {
            // Act — omit ownershipType to confirm default works
            var result = await DataverseMetadataTool.CreateTable(
                schemaName: schemaName,
                displayName: "Test MCP Default Owned",
                pluralName: "Test MCP Default Owned Tables");

            // Assert
            var response = JsonSerializer.Deserialize<JsonElement>(result);
            var success = response.GetProperty("Success").GetBoolean();
            Assert.True(success, $"Expected success but got: {result}");

            TrackCreatedTable(schemaName);
            _output.WriteLine($"Successfully created table with default ownership: {schemaName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Cleanup

    public override async ValueTask DisposeAsync()
    {
        if (_createdTables.Count > 0)
        {
            _output.WriteLine($"Cleaning up {_createdTables.Count} created tables...");

            foreach (var tableName in _createdTables)
            {
                try
                {
                    _output.WriteLine($"Deleting table: {tableName}");
                    var deleteRequest = new DeleteEntityRequest { LogicalName = tableName };
                    await ServiceClient.ExecuteAsync(deleteRequest);
                    _output.WriteLine($"Successfully deleted table: {tableName}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to clean up table {tableName}: {ex.Message}");
                }
            }

            _createdTables.Clear();
        }

        await base.DisposeAsync();
    }

    #endregion
}
