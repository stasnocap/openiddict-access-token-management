using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.ClientAccess;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;
using OpenIddict.AccessTokenManagement.UserAccess.Extensions;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess;

/// <summary>
/// Delegating handler that injects the current access token into an outgoing request
/// </summary>
public class OpenIddictUserAccessTokenHandler : AccessTokenHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserTokenRequestParameters _parameters;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="logger"></param>
    /// <param name="parameters"></param>
    public OpenIddictUserAccessTokenHandler(
        IOptions<ManagementOptions> managementOptions,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OpenIddictUserAccessTokenHandler> logger,
        UserTokenRequestParameters? parameters = null)
        : base(managementOptions, logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _parameters = parameters ?? new UserTokenRequestParameters();
    }

    /// <inheritdoc/>
    protected override async Task<ClientToken> GetAccessTokenAsync(bool forceRenewal, CancellationToken cancellationToken)
    {
        var parameters = new UserTokenRequestParameters
        {
            SignInScheme = _parameters.SignInScheme,
            ChallengeScheme = _parameters.ChallengeScheme,
            ForceRenewal = forceRenewal,
        };

        return await _httpContextAccessor.HttpContext!.GetUserAccessTokenAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}