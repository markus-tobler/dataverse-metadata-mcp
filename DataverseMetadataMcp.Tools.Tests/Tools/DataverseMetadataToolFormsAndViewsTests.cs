using System.Text.Json;
using DataverseMetadataMcp.Tools.Tools;
using Xunit;
using Xunit.Abstractions;

namespace DataverseMetadataMcp.Tools.Tests.Tools;

/// <summary>
/// Integration tests for CreateForm, UpdateForm, and the ReadFormDetails includeXml parameter.
/// </summary>
public class DataverseMetadataToolFormsAndViewsTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly List<Guid> _createdFormIds = new();

    public DataverseMetadataToolFormsAndViewsTests(ITestOutputHelper output) : base()
    {
        _output = output;
    }

    /// <summary>
    /// Builds minimal structurally-valid FormXML for a Main form on the account table.
    /// Uses fresh GUIDs for tab/cell IDs to avoid collisions between test runs.
    /// </summary>
    private static string BuildMinimalAccountFormXml()
    {
        string tabId = Guid.NewGuid().ToString("B").ToUpper();
        string cellId = Guid.NewGuid().ToString("B").ToUpper();
        return
            "<form>" +
            $"<tabs><tab name=\"tab_general\" id=\"{tabId}\" IsUserDefined=\"0\" expanded=\"true\">" +
            "<labels><label description=\"General\" languagecode=\"1033\" /></labels>" +
            "<columns><column width=\"100%\"><sections>" +
            "<section name=\"section_general\" showlabel=\"false\" showbar=\"false\" columns=\"111\">" +
            "<labels><label description=\"General\" languagecode=\"1033\" /></labels>" +
            "<rows><row>" +
            $"<cell auto=\"true\" id=\"{cellId}\">" +
            "<labels><label description=\"Account Name\" languagecode=\"1033\" /></labels>" +
            "<control id=\"name\" classid=\"{4273EDBD-AC1D-40d3-9FB2-095C621B552D}\"><parameters /></control>" +
            "</cell></row></rows>" +
            "</section></sections></column></columns></tab></tabs>" +
            "</form>";
    }

    private void TrackCreatedForm(Guid formId)
    {
        _createdFormIds.Add(formId);
        _output.WriteLine($"Tracked form for cleanup: {formId}");
    }

    #region CreateForm Validation Tests

    [Fact]
    public async Task CreateForm_EmptyTableName_ReturnsError()
    {
        var result = await DataverseMetadataTool.CreateForm(
            tableName: "",
            formName: "Test Form",
            formXml: "<form />");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("Error", out _), $"Expected 'Error' property. Got: {result}");
        _output.WriteLine($"Received expected error: {result}");
    }

    [Fact]
    public async Task CreateForm_EmptyFormName_ReturnsError()
    {
        var result = await DataverseMetadataTool.CreateForm(
            tableName: "account",
            formName: "",
            formXml: "<form />");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("Error", out _), $"Expected 'Error' property. Got: {result}");
        _output.WriteLine($"Received expected error: {result}");
    }

    [Fact]
    public async Task CreateForm_EmptyFormXml_ReturnsError()
    {
        var result = await DataverseMetadataTool.CreateForm(
            tableName: "account",
            formName: "Test Form",
            formXml: "");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("Error", out _), $"Expected 'Error' property. Got: {result}");
        _output.WriteLine($"Received expected error: {result}");
    }

    #endregion

    #region UpdateForm Validation Tests

    // UpdateForm fetches the serviceClient before performing validation,
    // so these tests require an active connection (consistent with UpdateView pattern).

    [Fact]
    public async Task UpdateForm_InvalidFormId_ReturnsError()
    {
        EnsureDataverseConnection();

        var result = await DataverseMetadataTool.UpdateForm(formId: "not-a-guid");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("Error", out _), $"Expected 'Error' property. Got: {result}");
        _output.WriteLine($"Received expected error: {result}");
    }

    [Fact]
    public async Task UpdateForm_NoFieldsProvided_ReturnsError()
    {
        EnsureDataverseConnection();

        // Valid GUID format, no update fields supplied — should fail before any Dataverse call
        var result = await DataverseMetadataTool.UpdateForm(formId: Guid.NewGuid().ToString());

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("Error", out _), $"Expected 'Error' property. Got: {result}");
        _output.WriteLine($"Received expected error: {result}");
    }

    #endregion

    #region CreateForm Integration Tests

    [Fact]
    public async Task CreateForm_MainForm_CreatesAndPublishesSuccessfully()
    {
        EnsureDataverseConnection();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var formName = $"MCP Test Main Form {timestamp}";

        try
        {
            var result = await DataverseMetadataTool.CreateForm(
                tableName: "account",
                formName: formName,
                formXml: BuildMinimalAccountFormXml(),
                description: "Created by integration test",
                formType: 2);

            _output.WriteLine($"CreateForm result: {result}");

            var response = JsonSerializer.Deserialize<JsonElement>(result);
            Assert.True(response.GetProperty("Success").GetBoolean(), $"Expected success. Got: {result}");
            Assert.Equal(formName, response.GetProperty("Name").GetString());
            Assert.Equal("account", response.GetProperty("TableName").GetString());
            Assert.Equal(2, response.GetProperty("FormType").GetInt32());
            Assert.Equal("Main", response.GetProperty("FormTypeName").GetString());
            Assert.True(response.TryGetProperty("FormId", out var formIdProp), "Expected 'FormId' in response");

            TrackCreatedForm(Guid.Parse(formIdProp.GetString()!));
        }
        catch (Exception ex) when (ex is not Xunit.Sdk.XunitException)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region ReadFormDetails Integration Tests

    [Fact]
    public async Task ReadFormDetails_DefaultBehavior_ExcludesFormXml()
    {
        EnsureDataverseConnection();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var createResult = await DataverseMetadataTool.CreateForm(
            tableName: "account",
            formName: $"MCP Test Read Form {timestamp}",
            formXml: BuildMinimalAccountFormXml());

        var createResponse = JsonSerializer.Deserialize<JsonElement>(createResult);
        Assert.True(createResponse.GetProperty("Success").GetBoolean(), $"Setup failed: {createResult}");

        var formId = Guid.Parse(createResponse.GetProperty("FormId").GetString()!);
        TrackCreatedForm(formId);

        // Act — default includeXml=false
        var result = await DataverseMetadataTool.ReadFormDetails(formId.ToString());
        _output.WriteLine($"ReadFormDetails (no XML): {result}");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("FormXmlLength", out _), "Expected 'FormXmlLength' in summary response");
        Assert.True(response.TryGetProperty("HasFormXml", out _), "Expected 'HasFormXml' in summary response");
        Assert.False(response.TryGetProperty("FormXml", out _), "'FormXml' should be absent when includeXml=false");
    }

    [Fact]
    public async Task ReadFormDetails_WithIncludeXml_ReturnsFormXml()
    {
        EnsureDataverseConnection();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var createResult = await DataverseMetadataTool.CreateForm(
            tableName: "account",
            formName: $"MCP Test Read XML Form {timestamp}",
            formXml: BuildMinimalAccountFormXml());

        var createResponse = JsonSerializer.Deserialize<JsonElement>(createResult);
        Assert.True(createResponse.GetProperty("Success").GetBoolean(), $"Setup failed: {createResult}");

        var formId = Guid.Parse(createResponse.GetProperty("FormId").GetString()!);
        TrackCreatedForm(formId);

        // Act — includeXml=true
        var result = await DataverseMetadataTool.ReadFormDetails(formId.ToString(), includeXml: true);
        _output.WriteLine($"ReadFormDetails (with XML): {result}");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.TryGetProperty("FormXml", out var formXmlProp), "Expected 'FormXml' when includeXml=true");
        Assert.False(string.IsNullOrEmpty(formXmlProp.GetString()), "FormXml should not be empty");
        Assert.False(response.TryGetProperty("FormXmlLength", out _), "'FormXmlLength' should be absent when includeXml=true");
    }

    #endregion

    #region UpdateForm Integration Tests

    [Fact]
    public async Task UpdateForm_UpdateName_ReturnsSuccess()
    {
        EnsureDataverseConnection();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var createResult = await DataverseMetadataTool.CreateForm(
            tableName: "account",
            formName: $"MCP Test Update Form {timestamp}",
            formXml: BuildMinimalAccountFormXml());

        var createResponse = JsonSerializer.Deserialize<JsonElement>(createResult);
        Assert.True(createResponse.GetProperty("Success").GetBoolean(), $"Setup failed: {createResult}");

        var formId = Guid.Parse(createResponse.GetProperty("FormId").GetString()!);
        TrackCreatedForm(formId);

        // Act
        var result = await DataverseMetadataTool.UpdateForm(
            formId: formId.ToString(),
            name: $"MCP Test Updated Name {timestamp}");

        _output.WriteLine($"UpdateForm (name) result: {result}");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.GetProperty("Success").GetBoolean(), $"Expected success. Got: {result}");
        var updatedFields = response.GetProperty("UpdatedFields").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("Name", updatedFields);
    }

    [Fact]
    public async Task UpdateForm_UpdateFormXml_ReturnsSuccess()
    {
        EnsureDataverseConnection();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var createResult = await DataverseMetadataTool.CreateForm(
            tableName: "account",
            formName: $"MCP Test Update XML Form {timestamp}",
            formXml: BuildMinimalAccountFormXml());

        var createResponse = JsonSerializer.Deserialize<JsonElement>(createResult);
        Assert.True(createResponse.GetProperty("Success").GetBoolean(), $"Setup failed: {createResult}");

        var formId = Guid.Parse(createResponse.GetProperty("FormId").GetString()!);
        TrackCreatedForm(formId);

        // Act — replace formXml with a fresh one
        var result = await DataverseMetadataTool.UpdateForm(
            formId: formId.ToString(),
            formXml: BuildMinimalAccountFormXml());

        _output.WriteLine($"UpdateForm (formXml) result: {result}");

        var response = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(response.GetProperty("Success").GetBoolean(), $"Expected success. Got: {result}");
        var updatedFields = response.GetProperty("UpdatedFields").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("FormXml", updatedFields);
    }

    #endregion

    #region Cleanup

    public override async ValueTask DisposeAsync()
    {
        if (_createdFormIds.Count > 0)
        {
            _output.WriteLine($"Cleaning up {_createdFormIds.Count} created form(s)...");
            foreach (var formId in _createdFormIds)
            {
                try
                {
                    await ServiceClient.DeleteAsync("systemform", formId);
                    _output.WriteLine($"Deleted form: {formId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Failed to delete form {formId}: {ex.Message}");
                }
            }
            _createdFormIds.Clear();
        }

        await base.DisposeAsync();
    }

    #endregion
}
