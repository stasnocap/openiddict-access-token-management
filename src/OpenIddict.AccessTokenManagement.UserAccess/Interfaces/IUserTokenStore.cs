using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Interfaces;

/// <summary>
/// Storage abstraction for access and refresh tokens
/// </summary>
public interface IUserTokenStore
{
    /// <summary>
    /// Stores tokens
    /// </summary>
    /// <param name="user">User the tokens belong to</param>
    /// <param name="token"></param>
    /// <param name="parameters">Extra optional parameters</param>
    /// <returns></returns>
    Task StoreTokenAsync(
        ClaimsPrincipal user,
        UserToken token,
        UserTokenRequestParameters? parameters = null);

    /// <summary>
    /// Retrieves tokens from store
    /// </summary>
    /// <param name="user">User the tokens belong to</param>
    /// <param name="parameters">Extra optional parameters</param>
    /// <returns>access and refresh token and access token expiration</returns>
    Task<UserToken> GetTokenAsync(
        ClaimsPrincipal user, 
        UserTokenRequestParameters? parameters = null);

    /// <summary>
    /// Retrieves tokens from authentication properties
    /// </summary>
    /// <param name="authenticationProperties">Authentication properties</param>
    /// <param name="parameters">Extra optional parameters</param>
    /// <returns></returns>
    UserToken GetTokenFromProperties(AuthenticationProperties authenticationProperties,
        UserTokenRequestParameters? parameters = null);

    /// <summary>
    /// Clears the stored tokens for a given user
    /// </summary>
    /// <param name="user">User the tokens belong to</param>
    /// <param name="parameters">Extra optional parameters</param>
    /// <returns></returns>
    Task ClearTokenAsync(
        ClaimsPrincipal user, 
        UserTokenRequestParameters? parameters = null);
}