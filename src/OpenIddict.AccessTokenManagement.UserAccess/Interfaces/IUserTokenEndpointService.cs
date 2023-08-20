using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Interfaces;

/// <summary>
/// Abstraction for token endpoint operations
/// </summary>
public interface IUserTokenEndpointService
{
    /// <summary>
    /// Refreshes a user access token.
    /// </summary>
    /// <param name="userToken"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserToken> RefreshAccessTokenAsync(
        UserToken userToken,
        UserTokenRequestParameters parameters,
        CancellationToken cancellationToken = default);
}