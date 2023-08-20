using System.Security.Claims;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Interfaces;

/// <summary>
/// Abstraction for managing user access tokens
/// </summary>
public interface IUserTokenManagementService
{
    /// <summary>
    /// Returns the user access token. If the current token is expired, it will try to refresh it.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserToken> GetAccessTokenAsync(
        ClaimsPrincipal user, 
        UserTokenRequestParameters? parameters = null, 
        CancellationToken cancellationToken = default);
}