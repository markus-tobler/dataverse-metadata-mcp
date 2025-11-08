using System.Text.Json;
using Microsoft.Extensions.Logging;
using DataverseMetadataMcp.Tools.Tools;
using Xunit;
using Xunit.Abstractions;

namespace DataverseMetadataMcp.Tools.Tests.Tools;

/// <summary>
/// Integration tests for DataverseMetadataTool plugin and custom API methods
/// </summary>
public class DataverseMetadataToolPluginsCustomApisTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public DataverseMetadataToolPluginsCustomApisTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ReadPluginAssemblies_ShouldReturnValidJson()
    {
        // Act
        var result = await DataverseMetadataTool.ReadPluginAssemblies();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result));

        // Verify it's valid JSON
        var assemblies = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(assemblies.ValueKind == JsonValueKind.Array);

        _output.WriteLine($"Found {assemblies.GetArrayLength()} plugin assemblies");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task ReadPluginAssemblies_ShouldContainExpectedProperties()
    {
        // Act
        var result = await DataverseMetadataTool.ReadPluginAssemblies();

        // Assert
        Assert.NotNull(result);

        var assemblies = JsonSerializer.Deserialize<JsonElement>(result);

        if (assemblies.GetArrayLength() > 0)
        {
            var firstAssembly = assemblies[0];

            // Check for expected properties
            Assert.True(firstAssembly.TryGetProperty("PluginAssemblyId", out _));
            Assert.True(firstAssembly.TryGetProperty("Name", out _));
            Assert.True(firstAssembly.TryGetProperty("Version", out _));
            Assert.True(firstAssembly.TryGetProperty("IsolationMode", out _));
            Assert.True(firstAssembly.TryGetProperty("SourceType", out _));
        }
    }

    [Fact]
    public async Task ReadPluginSteps_WithValidTableName_ShouldReturnValidJson()
    {
        // Arrange - using "account" as a common table that likely exists
        const string tableName = "account";        // Act
        var result = await DataverseMetadataTool.ReadPluginSteps(tableName);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result));

        // Verify it's valid JSON
        var steps = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(steps.ValueKind == JsonValueKind.Array);

        _output.WriteLine($"Found {steps.GetArrayLength()} plugin steps for table '{tableName}'");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task ReadPluginSteps_ShouldContainExpectedProperties()
    {
        // Arrange
        const string tableName = "account";

        // Act
        var result = await DataverseMetadataTool.ReadPluginSteps(tableName);

        // Assert
        Assert.NotNull(result);

        var steps = JsonSerializer.Deserialize<JsonElement>(result);

        if (steps.GetArrayLength() > 0)
        {
            var firstStep = steps[0];

            // Check for expected properties
            Assert.True(firstStep.TryGetProperty("StepId", out _));
            Assert.True(firstStep.TryGetProperty("Name", out _));
            Assert.True(firstStep.TryGetProperty("MessageName", out _));
            Assert.True(firstStep.TryGetProperty("Stage", out _));
            Assert.True(firstStep.TryGetProperty("Mode", out _));
            Assert.True(firstStep.TryGetProperty("Status", out _));
        }
    }

    [Fact]
    public async Task ReadCustomApis_ShouldReturnValidJson()
    {
        // Act
        var result = await DataverseMetadataTool.ReadCustomApis();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result));

        // Verify it's valid JSON
        var customApis = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(customApis.ValueKind == JsonValueKind.Array);

        _output.WriteLine($"Found {customApis.GetArrayLength()} custom APIs");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task ReadCustomApis_ShouldContainExpectedProperties()
    {
        // Act
        var result = await DataverseMetadataTool.ReadCustomApis();

        // Assert
        Assert.NotNull(result);

        var customApis = JsonSerializer.Deserialize<JsonElement>(result);

        if (customApis.GetArrayLength() > 0)
        {
            var firstApi = customApis[0];

            // Check for expected properties
            Assert.True(firstApi.TryGetProperty("CustomApiId", out _));
            Assert.True(firstApi.TryGetProperty("UniqueName", out _));
            Assert.True(firstApi.TryGetProperty("DisplayName", out _));
            Assert.True(firstApi.TryGetProperty("BindingType", out _));
            Assert.True(firstApi.TryGetProperty("IsFunction", out _));
            Assert.True(firstApi.TryGetProperty("IsPrivate", out _));
        }
    }

    [Fact]
    public async Task ReadCustomApiDetails_WithValidUniqueName_ShouldReturnValidJson()
    {
        // First get a list of custom APIs to find a valid unique name
        var customApisResult = await DataverseMetadataTool.ReadCustomApis();
        var customApis = JsonSerializer.Deserialize<JsonElement>(customApisResult);

        if (customApis.GetArrayLength() > 0)
        {
            var firstApi = customApis[0];
            var uniqueName = firstApi.GetProperty("UniqueName").GetString();
            Assert.NotNull(uniqueName);

            // Act
            var result = await DataverseMetadataTool.ReadCustomApiDetails(uniqueName);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result));

            // Verify it's valid JSON
            var apiDetails = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(apiDetails.ValueKind == JsonValueKind.Object);

            _output.WriteLine($"Custom API details for '{uniqueName}':");
            _output.WriteLine(result);
        }
        else
        {
            _output.WriteLine("No custom APIs found in the environment, skipping detailed test");
        }
    }

    [Fact]
    public async Task ReadCustomApiDetails_WithInvalidUniqueName_ShouldReturnNotFoundMessage()
    {
        // Arrange
        const string invalidUniqueName = "nonexistent_custom_api_12345";

        // Act
        var result = await DataverseMetadataTool.ReadCustomApiDetails(invalidUniqueName);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("not found", result, StringComparison.OrdinalIgnoreCase);

        _output.WriteLine($"Result for invalid unique name: {result}");
    }

    [Fact]
    public async Task ReadPluginTypes_ShouldReturnValidJson()
    {
        // Act
        var result = await DataverseMetadataTool.ReadPluginTypes();

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result));

        // Verify it's valid JSON
        var pluginTypes = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(pluginTypes.ValueKind == JsonValueKind.Array);

        _output.WriteLine($"Found {pluginTypes.GetArrayLength()} plugin types");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task ReadPluginTypes_ShouldContainExpectedProperties()
    {
        // Act
        var result = await DataverseMetadataTool.ReadPluginTypes();

        // Assert
        Assert.NotNull(result);

        var pluginTypes = JsonSerializer.Deserialize<JsonElement>(result);

        if (pluginTypes.GetArrayLength() > 0)
        {
            var firstType = pluginTypes[0];

            // Check for expected properties
            Assert.True(firstType.TryGetProperty("PluginTypeId", out _));
            Assert.True(firstType.TryGetProperty("Name", out _));
            Assert.True(firstType.TryGetProperty("TypeName", out _));
            Assert.True(firstType.TryGetProperty("AssemblyName", out _));
            Assert.True(firstType.TryGetProperty("IsWorkflowActivity", out _));
        }
    }

    [Theory]
    [InlineData("contact")]
    [InlineData("account")]
    public async Task ReadPluginSteps_WithDifferentTableNames_ShouldReturnValidJson(string tableName)
    {
        // Act
        var result = await DataverseMetadataTool.ReadPluginSteps(tableName);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result));

        // Verify it's valid JSON
        var steps = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(steps.ValueKind == JsonValueKind.Array);

        _output.WriteLine($"Found {steps.GetArrayLength()} plugin steps for table '{tableName}'");
    }

    [Fact]
    public async Task ReadPluginSteps_WithNonExistentTable_ShouldReturnEmptyArray()
    {
        // Arrange
        const string nonExistentTable = "nonexistenttable12345";

        // Act
        var result = await DataverseMetadataTool.ReadPluginSteps(nonExistentTable);

        // Assert
        Assert.NotNull(result);

        // Should either return empty array or error message
        if (result.StartsWith("["))
        {
            var steps = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(steps.ValueKind == JsonValueKind.Array);
            Assert.Equal(0, steps.GetArrayLength());
        }
        else
        {
            // Should contain error message
            Assert.Contains("Error", result, StringComparison.OrdinalIgnoreCase);
        }

        _output.WriteLine($"Result for non-existent table: {result}");
    }
}
