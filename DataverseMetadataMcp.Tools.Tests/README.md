# DataverseMetadataMcp.Tools Tests

This test project provides comprehensive testing for the DataverseMetadataMcp.Tools library, focusing on the Dataverse metadata retrieval functionality with **real Dataverse connections** (no mocking).

## ✅ Project Status

The `DataverseMetadataMcp.Tools.Tests` project has been successfully created with comprehensive integration tests for the DataverseMetadataTool.PluginsCustomApis functionality.

## 📁 Project Structure

```txt
DataverseMetadataMcp.Tools.Tests/
├── DataverseMetadataMcp.Tools.Tests.csproj          # Test project file
├── README.md                                 # Comprehensive documentation
├── TestBase.cs                               # Base class for integration tests
├── appsettings.example.json                  # Example configuration
├── Configuration/
│   └── ConfigurationHelperTests.cs          # Unit tests for ConfigurationHelper
└── Tools/
    └── DataverseMetadataToolPluginsCustomApisTests.cs  # Integration tests
```

## 🔧 Key Features

### Real Dataverse Integration (No Mocking)

- Uses actual Dataverse connections for authentic testing
- Shared connection management across all tests for performance
- Automatic test skipping when connection is not available

### Comprehensive Test Coverage

- ✅ `ReadPluginAssemblies()` - Tests plugin assembly retrieval
- ✅ `ReadPluginSteps(tableName)` - Tests plugin steps for specific tables
- ✅ `ReadCustomApis()` - Tests custom API retrieval
- ✅ `ReadCustomApiDetails(uniqueName)` - Tests detailed custom API information
- ✅ `ReadPluginTypes()` - Tests plugin type retrieval

### Smart Test Behavior

- JSON validation and structure verification
- Error handling scenarios with invalid inputs
- Tests with multiple table names (account, contact, opportunity, lead)
- Graceful handling of empty result sets

## Test Structure

### Infrastructure

- **TestBase**: Base class that provides shared connection management and dependency injection setup for integration tests
- **SkipException**: Custom exception to skip tests when Dataverse connection is not available

### Tools Tests

- **DataverseMetadataToolPluginsCustomApisTests**: Comprehensive tests for plugin and custom API metadata methods
- **ConfigurationHelperTests**: Unit tests for the configuration helper initialization

## Test Configuration

### Connection String Setup

The tests require a valid Dataverse connection string. You can provide this in several ways:

1. **Environment Variable** (recommended for CI/CD):

   ```powershell
   $env:DATAVERSE_CONNECTION_STRING = "AuthType=OAuth;Url=https://yourorg.crm4.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/"
   ```

2. **User Secrets** (recommended for local development):

   ```powershell
   dotnet user-secrets init --project DataverseMetadataMcp.Tools.Tests
   dotnet user-secrets set "ConnectionString" "AuthType=OAuth;Url=https://yourorg.crm4.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/" --project DataverseMetadataMcp.Tools.Tests
   ```

### Connection String Format

Use the standard Dataverse connection string format:

```txt
AuthType=OAuth;Url=https://yourorg.crm4.dynamics.com/;Username=your@email.com;ClientId=your-client-id;LoginPrompt=Auto;RedirectUri=http://localhost/
```

For more connection string options, see the [Microsoft Power Platform Dataverse documentation](https://docs.microsoft.com/en-us/powerapps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect).

## Running Tests

### Prerequisites

1. **Valid Dataverse Environment** with metadata to test
2. **User Permissions**: System Administrator or System Customizer role
3. **Network Access** to Dataverse environment
4. **Valid Connection String** configured

### Commands

```powershell
# Run all tests
dotnet test

# Run with verbose output
dotnet test -v normal

# Run specific test class
dotnet test --filter "DataverseMetadataToolPluginsCustomApisTests"

# Run specific test method
dotnet test --filter "ReadPluginAssemblies_ShouldReturnValidJson"

# Using VS Code tasks
# Ctrl+Shift+P -> "Tasks: Run Task" -> "test" or "test-all"
```

## Test Features

### Shared Connection Management

- Single connection instance shared across all tests for performance
- Automatic connection validation during test setup
- Proper resource cleanup
- Graceful handling of connection failures with test skipping

### Comprehensive Coverage

- Tests for all plugin and custom API metadata methods:
  - `ReadPluginAssemblies()` - Tests retrieval of plugin assemblies
  - `ReadPluginSteps(tableName)` - Tests plugin steps for specific tables
  - `ReadCustomApis()` - Tests custom API retrieval
  - `ReadCustomApiDetails(uniqueName)` - Tests detailed custom API information
  - `ReadPluginTypes()` - Tests plugin type retrieval
- JSON validation and structure verification
- Error handling scenarios
- Edge case testing with invalid inputs

### Integration Testing

- **Real Dataverse connections** for authentic testing
- Validates actual data structures returned from Dataverse
- Tests with different table names and scenarios (account, contact, opportunity, lead)
- Handles environments with varying data gracefully

## Test Data Expectations

The integration tests work with real Dataverse environments and will:

- Return actual plugin assemblies, types, and steps from your environment
- Test with common tables like 'account', 'contact', 'opportunity', 'lead'
- Handle environments with no custom APIs gracefully
- Validate JSON structure and expected properties even with empty result sets

## Troubleshooting

### Common Issues

1. **Connection String Not Found**

   ```txt
   SkipException: No Dataverse connection string found...
   ```

   - Ensure environment variable `DATAVERSE_CONNECTION_STRING` is set, or
   - Add connection string to user secrets with key `ConnectionString`

2. **Authentication Failures**

   ```txt
   SkipException: Failed to establish connection to Dataverse...
   ```

   - Verify connection string format
   - Check that the user has appropriate permissions
   - For OAuth, ensure the redirect URI is correct
   - Try the connection string in other tools first

3. **Test Timeout**

   - Dataverse connections can be slow on first connection
   - Authentication prompts may appear during test runs
   - Consider increasing test timeout in your IDE

4. **Insufficient Permissions**

   ```txt
   Error retrieving plugin assemblies: Access Denied...
   ```

   - Ensure the user has System Administrator or System Customizer roles
   - Verify read permissions on system entities

### GitHub Actions / CI/CD

The tests are designed to work seamlessly in GitHub Actions:

1. **Set GitHub Secret**: Add `DATAVERSE_CONNECTION_STRING` as a repository secret
2. **Workflow Configuration**: Your GitHub Actions workflow should set the environment variable:

   ```yaml
   - name: Test
     env:
       DATAVERSE_CONNECTION_STRING: ${{ secrets.DATAVERSE_CONNECTION_STRING }}
     run: dotnet test --configuration Release --no-build --verbosity normal
   ```

3. **Test Behavior**: Tests automatically skip when connection is unavailable (graceful degradation)
4. **Logs**: Connection attempts and validation results are logged for debugging

### Debug Tips

- Tests output detailed information using `ITestOutputHelper`
- Check test output for actual JSON responses and connection details
- Use `-v normal` flag for more detailed test execution logs
- Set breakpoints in test methods to inspect actual Dataverse data
- Tests will show the number of entities found (useful for understanding your environment)

### Example Test Output

```txt
Found 12 plugin assemblies
Found 45 plugin steps for table 'account'
Found 3 custom APIs
Custom API details for 'my_custom_api':
{
  "CustomApiId": "...",
  "UniqueName": "my_custom_api",
  ...
}
```

## 📊 Test Output Example

When tests run successfully, you'll see output like:

```text
Found 12 plugin assemblies
Found 45 plugin steps for table 'account'
Found 3 custom APIs
Custom API details for 'my_custom_api': { ... }
```

## 🛠️ Technical Implementation

### TestBase Class

- Manages shared Dataverse connection across all tests
- Provides dependency injection setup
- Handles connection failures gracefully with `SkipException`
- Proper resource cleanup

### Integration Test Features

- Real Dataverse API calls (no mocking)
- JSON schema validation
- Property existence verification
- Error condition testing
- Multi-environment compatibility

## ✨ Benefits

1. **Authentic Testing**: Uses real Dataverse data and APIs
2. **Performance**: Shared connection reduces test execution time
3. **Robustness**: Handles various environment configurations
4. **Debugging**: Detailed output helps understand your Dataverse environment
5. **Maintainable**: Clean architecture with proper separation of concerns

## Contributing

When adding new tests:

1. Inherit from `TestBase` for integration tests
2. Call `EnsureDataverseConnection()` at the start of test methods
3. Use `_output.WriteLine()` to provide useful debug information
4. Handle cases where Dataverse entities might not exist in all environments
5. Test both success and error scenarios

## 🎯 Project Summary

The test project is now ready for use and provides comprehensive coverage of the plugin and custom API metadata functionality! The project has been successfully created with:

- ✅ **Real Dataverse Integration** with no mocking for authentic testing
- ✅ **Comprehensive Test Coverage** for all plugin and custom API methods
- ✅ **Smart Test Management** with automatic connection handling and test skipping
- ✅ **Performance Optimizations** through shared connection management
- ✅ **Robust Error Handling** for various environment configurations
- ✅ **Developer-Friendly** setup with detailed documentation and troubleshooting guides

The integration tests work seamlessly with real Dataverse environments, providing valuable insights into your actual metadata while maintaining clean, maintainable test code.
