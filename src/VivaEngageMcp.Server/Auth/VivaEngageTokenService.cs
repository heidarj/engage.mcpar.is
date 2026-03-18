using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using VivaEngageMcp.Server.Configuration;
using Microsoft.Extensions.Options;

namespace VivaEngageMcp.Server.Auth;

/// <summary>
/// Acquires delegated Viva Engage (Yammer) tokens via OBO flow.
/// The downstream resource/scope is configurable via <see cref="VivaEngageOptions"/>.
/// </summary>
public sealed class VivaEngageTokenService : IVivaEngageTokenService
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly VivaEngageOptions _vivaEngageOptions;
    private readonly ILogger<VivaEngageTokenService> _logger;

    public VivaEngageTokenService(
        ITokenAcquisition tokenAcquisition,
        IOptions<VivaEngageOptions> vivaEngageOptions,
        ILogger<VivaEngageTokenService> logger)
    {
        _tokenAcquisition = tokenAcquisition;
        _vivaEngageOptions = vivaEngageOptions.Value;
        _logger = logger;
    }

    public async Task<string> GetVivaEngageAccessTokenForUserAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Acquiring Viva Engage access token on behalf of user.");
        try
        {
            var scopes = new[] { _vivaEngageOptions.RequestedScopeOrResource };
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(
                scopes,
                user: user);
            return token;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _logger.LogWarning("Viva Engage token acquisition requires user interaction: {Message}", ex.Message);
            throw new VivaEngageAuthException("Viva Engage token acquisition requires user interaction (consent or re-auth).", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire Viva Engage access token.");
            throw new VivaEngageAuthException("Failed to acquire Viva Engage access token.", ex);
        }
    }
}
