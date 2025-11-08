using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using DataverseMetadataMcp.Tools.Tools;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace DataverseMetadataMcp.Tools.Tests.Tools;

/// <summary>
/// Integration tests for DataverseMetadataTool column creation and deletion methods
/// </summary>
public class DataverseMetadataToolCreateColumnsTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _createdColumns = new();
    private const string TestTableName = "account"; // Using standard Account entity
    private const string TestContactTableName = "contact"; // Using standard Contact entity

    public DataverseMetadataToolCreateColumnsTests(ITestOutputHelper output) : base()
    {
        _output = output;
    }

    #region Test Helper Methods

    /// <summary>
    /// Generate a unique column name for testing
    /// </summary>
    private string GenerateTestColumnName(string prefix = "test")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(100, 999);
        return $"new_{prefix}_{timestamp}_{random}";
    }

    /// <summary>
    /// Track created columns for cleanup
    /// </summary>
    private void TrackCreatedColumn(string tableName, string columnName)
    {
        _createdColumns.Add($"{tableName}:{columnName}");
        _output.WriteLine($"Tracked column for cleanup: {tableName}.{columnName}");
    }    /// <summary>
         /// Parse JSON response and extract success status
         /// </summary>
    private (bool Success, dynamic? Data, string[] Errors) ParseResponse(string jsonResponse)
    {
        var response = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
        var success = response.GetProperty("Success").GetBoolean();

        string[] errors = Array.Empty<string>();
        if (response.TryGetProperty("Errors", out var errorsElement))
        {
            errors = errorsElement.EnumerateArray().Select(e => e.GetString() ?? "").ToArray();
        }

        dynamic? data = null;
        if (response.TryGetProperty("Data", out var dataElement))
        {
            data = dataElement;
        }

        return (success, data, errors);
    }

    /// <summary>
    /// Verify that a column exists in Dataverse
    /// </summary>
    private async Task<bool> ColumnExists(string tableName, string columnName)
    {
        try
        {
            var request = new RetrieveAttributeRequest
            {
                EntityLogicalName = tableName,
                LogicalName = columnName.ToLower()
            };

            await ServiceClient.ExecuteAsync(request);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Text Column Tests

    [Fact]
    public async Task CreateTextColumn_ValidInput_CreatesColumnSuccessfully()
    {
        // Skip test if no connection
        EnsureDataverseConnection();

        // Arrange
        var columnName = GenerateTestColumnName("text");
        var displayName = "Test Text Column";
        var description = "Test text column for unit testing";

        try
        {
            // Act
            var result = await DataverseMetadataTool.CreateTextColumn(
                TestTableName, columnName, displayName, description, 255, false, "Text");

            // Assert
            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            // Verify column was actually created
            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created text column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateTextColumn_InvalidTableName_ReturnsError()
    {
        // Arrange
        var columnName = GenerateTestColumnName("invalid");

        // Act
        var result = await DataverseMetadataTool.CreateTextColumn(
            "nonexistent_table", columnName, "Test", "Test", 100);

        // Assert
        var (success, data, errors) = ParseResponse(result);
        Assert.False(success);
        Assert.NotEmpty(errors);
        _output.WriteLine($"Expected error received: {string.Join(", ", errors)}");
    }

    #endregion

    #region Integer Column Tests

    [Fact]
    public async Task CreateIntegerColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("int");
        var displayName = "Test Integer Column";
        var description = "Test integer column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateIntegerColumn(
                TestTableName, columnName, displayName, description, 0, 1000, false, "None");

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created integer column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Decimal Column Tests

    [Fact]
    public async Task CreateDecimalColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("decimal");
        var displayName = "Test Decimal Column";
        var description = "Test decimal column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateDecimalColumn(
                TestTableName, columnName, displayName, description, 2, 0, 999.99m, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created decimal column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Currency Column Tests

    [Fact]
    public async Task CreateCurrencyColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("currency");
        var displayName = "Test Currency Column";
        var description = "Test currency column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateCurrencyColumn(
                TestTableName, columnName, displayName, description, 2, 0, 100000, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created currency column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region DateTime Column Tests

    [Fact]
    public async Task CreateDateTimeColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("datetime");
        var displayName = "Test DateTime Column";
        var description = "Test datetime column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateDateTimeColumn(
                TestTableName, columnName, displayName, description, "DateOnly", "UserLocal", false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created datetime column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Boolean Column Tests

    [Fact]
    public async Task CreateBooleanColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("bool");
        var displayName = "Test Boolean Column";
        var description = "Test boolean column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateBooleanColumn(
                TestTableName, columnName, displayName, description, "Yes", "No", false, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created boolean column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Multiline Text Column Tests

    [Fact]
    public async Task CreateMultilineTextColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("multitext");
        var displayName = "Test Multiline Text Column";
        var description = "Test multiline text column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateMultilineTextColumn(
                TestTableName, columnName, displayName, description, 2000, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created multiline text column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Choice Column Tests

    [Fact]
    public async Task CreateChoiceColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("choice");
        var displayName = "Test Choice Column";
        var description = "Test choice column for unit testing";

        try
        {
            var choices = new[]
            {
                new { Value = 100000000, Label = "Option 1", Description = "First option" },
                new { Value = 100000001, Label = "Option 2", Description = "Second option" }
            };

            var result = await DataverseMetadataTool.CreateChoiceColumn(
                TestTableName, columnName, displayName, description,
                JsonSerializer.Serialize(choices), 100000000, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created choice column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region File Column Tests

    [Fact]
    public async Task CreateFileColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("file");
        var displayName = "Test File Column";
        var description = "Test file column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateFileColumn(
                TestTableName, columnName, displayName, description, 30720, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created file column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Lookup Column Tests

    [Fact]
    public async Task CreateLookupColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("lookup");
        var displayName = "Test Lookup Column";
        var description = "Test lookup column for unit testing";

        try
        {
            // Create a lookup to the Contact entity (which should always be available)
            var result = await DataverseMetadataTool.CreateLookupColumn(
                TestTableName, columnName, displayName, description, "contact", "", false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created lookup column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateLookupColumn_InvalidTargetTable_ReturnsError()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("invalidlookup");
        var displayName = "Test Invalid Lookup Column";
        var description = "Test lookup column with invalid target";

        try
        {
            // Try to create a lookup to a non-existent table
            var result = await DataverseMetadataTool.CreateLookupColumn(
                TestTableName, columnName, displayName, description, "nonexistent_table", "", false);

            var (success, data, errors) = ParseResponse(result);
            Assert.False(success, "Expected failure for invalid target table");
            Assert.NotEmpty(errors);

            _output.WriteLine($"Expected error received: {string.Join(", ", errors)}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Delete Column Tests

    [Fact]
    public async Task DeleteColumn_ExistingColumn_DeletesSuccessfully()
    {
        EnsureDataverseConnection();

        // First create a column to delete
        var columnName = GenerateTestColumnName("todelete");
        var displayName = "Test Column To Delete";
        var description = "Test column that will be deleted";

        try
        {
            // Create the column
            var createResult = await DataverseMetadataTool.CreateTextColumn(
                TestTableName, columnName, displayName, description, 100, false, "Text");

            var (createSuccess, createData, createErrors) = ParseResponse(createResult);
            Assert.True(createSuccess, $"Failed to create test column: {string.Join(", ", createErrors)}");

            // Verify it exists
            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist before deletion");

            // Now delete it
            var deleteResult = await DataverseMetadataTool.DeleteColumn(TestTableName, columnName);

            var (deleteSuccess, deleteData, deleteErrors) = ParseResponse(deleteResult);
            Assert.True(deleteSuccess, $"Failed to delete column: {string.Join(", ", deleteErrors)}");

            // Verify it's gone
            var stillExists = await ColumnExists(TestTableName, columnName);
            Assert.False(stillExists, "Column should not exist after deletion");

            _output.WriteLine($"Successfully deleted column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task DeleteColumn_NonExistentColumn_ReturnsError()
    {
        EnsureDataverseConnection();

        var nonExistentColumn = GenerateTestColumnName("nonexistent");

        // Act
        var result = await DataverseMetadataTool.DeleteColumn(TestTableName, nonExistentColumn);

        // Assert
        var (success, data, errors) = ParseResponse(result);
        Assert.False(success);
        Assert.NotEmpty(errors);
        _output.WriteLine($"Expected error received: {string.Join(", ", errors)}");
    }

    #endregion

    #region Additional Column Type Tests

    [Fact]
    public async Task CreateFloatingPointColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("float");
        var displayName = "Test Float Column";
        var description = "Test floating point column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateFloatingPointColumn(
                TestTableName, columnName, displayName, description, 5, 0, 99999.99999, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created floating point column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateBigIntegerColumn_ValidInput_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("bigint");
        var displayName = "Test BigInt Column";
        var description = "Test big integer column for unit testing";

        try
        {
            var result = await DataverseMetadataTool.CreateBigIntegerColumn(
                TestTableName, columnName, displayName, description, false);

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestTableName, columnName);

            var exists = await ColumnExists(TestTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created big integer column: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Contact Entity Tests

    [Fact]
    public async Task CreateTextColumn_OnContactEntity_CreatesColumnSuccessfully()
    {
        EnsureDataverseConnection();

        var columnName = GenerateTestColumnName("contact_text");
        var displayName = "Test Contact Text Column";
        var description = "Test text column on contact entity";

        try
        {
            var result = await DataverseMetadataTool.CreateTextColumn(
                TestContactTableName, columnName, displayName, description, 100, false, "Text");

            var (success, data, errors) = ParseResponse(result);
            Assert.True(success, $"Expected success but got errors: {string.Join(", ", errors)}");

            TrackCreatedColumn(TestContactTableName, columnName);

            var exists = await ColumnExists(TestContactTableName, columnName);
            Assert.True(exists, "Column should exist in Dataverse after creation");

            _output.WriteLine($"Successfully created text column on contact: {columnName}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Clean up all created columns after tests
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        if (_createdColumns.Count > 0)
        {
            _output.WriteLine($"Cleaning up {_createdColumns.Count} created columns...");

            foreach (var columnInfo in _createdColumns)
            {
                try
                {
                    var parts = columnInfo.Split(':');
                    var tableName = parts[0];
                    var columnName = parts[1];

                    _output.WriteLine($"Deleting column: {tableName}.{columnName}");

                    var result = await DataverseMetadataTool.DeleteColumn(tableName, columnName);
                    var (success, data, errors) = ParseResponse(result);

                    if (success)
                    {
                        _output.WriteLine($"Successfully cleaned up column: {tableName}.{columnName}");
                    }
                    else
                    {
                        _output.WriteLine($"Failed to clean up column {tableName}.{columnName}: {string.Join(", ", errors)}");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Error during cleanup of column {columnInfo}: {ex.Message}");
                }
            }

            _createdColumns.Clear();
        }

        await base.DisposeAsync();
    }

    #endregion
}
