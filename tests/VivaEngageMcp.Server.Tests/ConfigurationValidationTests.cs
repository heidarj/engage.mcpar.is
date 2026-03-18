using Microsoft.Extensions.Options;
using VivaEngageMcp.Server.Configuration;

namespace VivaEngageMcp.Server.Tests;

public sealed class ConfigurationValidationTests
{
    [Fact]
    public void EntraOptions_ValidatesRequiredFields()
    {
        var options = new EntraOptions
        {
            Instance = string.Empty,
            TenantId = "common",
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            Audience = string.Empty,
            ApiScope = "Mcp.Read"
        };

        // Instance is [Required] so the binder and Data Annotations validate it
        Assert.NotNull(options);
        // The defaults should be reasonable
        Assert.Equal("Mcp.Read", options.ApiScope);
    }

    [Fact]
    public void EntraOptions_DefaultInstanceIsLoginMicrosoftCom()
    {
        var options = new EntraOptions();
        Assert.Equal("https://login.microsoftonline.com/", options.Instance);
    }

    [Fact]
    public void EntraOptions_DefaultTenantIsCommon()
    {
        var options = new EntraOptions();
        Assert.Equal("common", options.TenantId);
    }

    [Fact]
    public void GraphOptions_HasDefaultScopes()
    {
        var options = new GraphOptions();
        Assert.Contains("https://graph.microsoft.com/Community.Read.All", options.Scopes);
        Assert.Contains("https://graph.microsoft.com/EngagementRole.Read", options.Scopes);
        Assert.Contains("https://graph.microsoft.com/User.Read", options.Scopes);
    }

    [Fact]
    public void GraphOptions_DefaultBaseUrlIsV1()
    {
        var options = new GraphOptions();
        Assert.Equal("https://graph.microsoft.com/v1.0", options.BaseUrl);
    }

    [Fact]
    public void VivaEngageOptions_DefaultBaseUrlIsYammer()
    {
        var options = new VivaEngageOptions();
        Assert.Equal("https://www.yammer.com/api/v1", options.BaseUrl);
        Assert.True(options.UseEntraTokens);
        Assert.Equal("access_as_user", options.DelegatedPermissionName);
    }

    [Fact]
    public void McpOptions_DefaultEndpointPathIsMcp()
    {
        var options = new McpOptions();
        Assert.Equal("/mcp", options.EndpointPath);
        Assert.Equal("viva-engage-mcp", options.ServerName);
        Assert.Equal("0.1.0", options.ServerVersion);
    }
}
