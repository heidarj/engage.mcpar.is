using System.Security.Claims;

namespace VivaEngageMcp.Server.Auth;

/// <summary>
/// Acquires delegated Microsoft Graph access tokens on behalf of the signed-in user.
/// </summary>
public interface IGraphTokenService
{
    Task<string> GetGraphAccessTokenForUserAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
