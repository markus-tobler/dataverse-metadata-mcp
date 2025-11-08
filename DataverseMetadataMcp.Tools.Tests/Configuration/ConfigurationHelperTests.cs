using DataverseMetadataMcp.Tools.Configuration;
using Xunit;

namespace DataverseMetadataMcp.Tools.Tests.Configuration;

/// <summary>
/// Unit tests for ConfigurationHelper initialization
/// </summary>
public class ConfigurationHelperTests
{
    [Fact]
    public void GetServiceClient_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        // Note: This test assumes ConfigurationHelper hasn't been initialized yet
        // In a real test run, the TestBase will initialize it, so this test might not be reliable
        // in a full test suite run

        try
        {
            // Reset to null to test uninitialized state
            ConfigurationHelper.Initialize(null!);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => ConfigurationHelper.GetServiceClient());
            Assert.Equal("Configuration not initialized", exception.Message);
        }
        catch (SkipException)
        {
            // If we're in a test suite that has already initialized the connection, skip this test
            return;
        }
    }

    [Fact]
    public void GetConnectionString_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        try
        {
            // Reset to null to test uninitialized state
            ConfigurationHelper.Initialize(null!);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => ConfigurationHelper.GetConnectionString());
            Assert.Equal("Configuration not initialized", exception.Message);
        }
        catch (SkipException)
        {
            // If we're in a test suite that has already initialized the connection, skip this test
            return;
        }
    }
}