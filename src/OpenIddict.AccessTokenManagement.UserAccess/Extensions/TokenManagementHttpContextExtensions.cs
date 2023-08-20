using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.AccessTokenManagement.UserAccess.Interfaces;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Extensions;

/// <summary>
/// Extensions methods for HttpContext for token management
/// </summary>
public static class TokenManagementHttpContextExtensions
{
    /// <summary>
    /// Returns (and refreshes if needed) the current access token for the logged on user
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <param name="parameters">Extra optional parameters</param>
    /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
    /// <returns></returns>
    public static async Task<UserToken> GetUserAccessTokenAsync(
        this HttpContext httpContext,
        UserTokenRequestParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var service = httpContext.RequestServices.GetRequiredService<IUserTokenManagementService>();

        return await service.GetAccessTokenAsync(httpContext.User, parameters, cancellationToken).ConfigureAwait(false);
    }
}