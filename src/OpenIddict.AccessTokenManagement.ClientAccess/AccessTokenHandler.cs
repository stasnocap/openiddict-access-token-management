using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;
using OpenIddict.AccessTokenManagement.ClientAccess.Unauthorized;
using static IdentityModel.OidcConstants;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Delegating handler that injects access token into an outgoing request
/// </summary>
public abstract class AccessTokenHandler : DelegatingHandler
{
    private readonly ManagementOptions _managementOptions;
    private readonly ILogger _logger;
    
    public AccessTokenHandler(
        IOptions<ManagementOptions> managementOptions,
        ILogger logger)
    {
        _managementOptions = managementOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Returns the access token for the outbound call.
    /// </summary>
    /// <param name="forceRenewal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<ClientToken> GetAccessTokenAsync(bool forceRenewal, CancellationToken cancellationToken);

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await SetTokenAsync(request, forceRenewal: false, cancellationToken).ConfigureAwait(false);
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // retry if 401
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();

            await SetTokenAsync(request, forceRenewal: true, cancellationToken).ConfigureAwait(false);

            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (_managementOptions.UseUnauthorizedMiddleware && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            return response;
        }

        return response;
    }

    /// <summary>
    /// Set an access token on the HTTP request
    /// </summary>
    /// <returns></returns>
    protected virtual async Task SetTokenAsync(HttpRequestMessage request, bool forceRenewal, CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(forceRenewal, cancellationToken).ConfigureAwait(false);
        
        if (!string.IsNullOrWhiteSpace(token?.AccessToken))
        {
            _logger.LogDebug("Sending access token in request to endpoint: {url}", request.RequestUri?.AbsoluteUri);

            var scheme = token.AccessTokenType ?? AuthenticationSchemes.AuthorizationHeaderBearer;

            // since AccessTokenType above in the token endpoint response (the token_type value) could be case insensitive, but
            // when we send it as an Authoriization header in the API request it must be case sensitive, we 
            // are checking for that here and forcing it to the exact casing required.
            if (scheme.Equals(AuthenticationSchemes.AuthorizationHeaderBearer, System.StringComparison.OrdinalIgnoreCase))
            {
                scheme = AuthenticationSchemes.AuthorizationHeaderBearer;
            }

            // checking for null AccessTokenType and falling back to "Bearer" since this might be coming
            // from an old cache/store prior to adding the AccessTokenType property.
            request.SetToken(scheme, token.AccessToken);
        }
    }
}