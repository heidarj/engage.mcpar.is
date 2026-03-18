using System.Security.Claims;

namespace VivaEngageMcp.Server.Auth;

/// <summary>
/// Acquires delegated Viva Engage (Yammer) access tokens on behalf of the signed-in user.
/// </summary>
public interface IVivaEngageTokenService
{
    Task<string> GetVivaEngageAccessTokenForUserAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
