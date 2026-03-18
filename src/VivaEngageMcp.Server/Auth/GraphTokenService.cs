using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using VivaEngageMcp.Server.Configuration;
using Microsoft.Extensions.Options;

namespace VivaEngageMcp.Server.Auth;

/// <summary>
/// Acquires delegated Graph tokens via OBO (on-behalf-of) flow using Microsoft.Identity.Web.
/// </summary>
public sealed class GraphTokenService : IGraphTokenService
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly GraphOptions _graphOptions;
    private readonly ILogger<GraphTokenService> _logger;

    public GraphTokenService(
        ITokenAcquisition tokenAcquisition,
        IOptions<GraphOptions> graphOptions,
        ILogger<GraphTokenService> logger)
    {
        _tokenAcquisition = tokenAcquisition;
        _graphOptions = graphOptions.Value;
        _logger = logger;
    }

    public async Task<string> GetGraphAccessTokenForUserAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Acquiring Graph access token on behalf of user.");
        try
        {
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(
                _graphOptions.Scopes,
                user: user);
            return token;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _logger.LogWarning("Graph token acquisition requires user interaction: {Message}", ex.Message);
            throw new VivaEngageAuthException("Graph token acquisition requires user interaction (consent or re-auth).", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire Graph access token.");
            throw new VivaEngageAuthException("Failed to acquire Graph access token.", ex);
        }
    }
}
