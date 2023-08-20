using Microsoft.Extensions.Logging;
using OpenIddict.AccessTokenManagement.ClientAccess.Extensions;
using OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;
using OpenIddict.Client;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Implements token endpoint operations using IdentityModel
/// </summary>
public class ClientTokenEndpointService : IClientTokenEndpointService
{
    private readonly OpenIddictClientService _openIddictClientService;
    private readonly ILogger<ClientTokenEndpointService> _logger;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="openIddictClientService"></param>
    /// <param name="logger"></param>
    public ClientTokenEndpointService(
        OpenIddictClientService openIddictClientService,
        ILogger<ClientTokenEndpointService> logger)
    {
        _openIddictClientService = openIddictClientService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public virtual async Task<ClientToken> RequestToken(
        string clientName,
        TokenRequestParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var request = parameters == null
            ? new()
            : new OpenIddictClientModels.ClientCredentialsAuthenticationRequest
            {
                Issuer = parameters.Issuer,
                CancellationToken = cancellationToken,
                Properties = parameters.Properties,
                ProviderName = parameters.ProviderName,
                RegistrationId = parameters.RegistrationId,
                AdditionalTokenRequestParameters = parameters.AdditionalTokenRequestParameters,
                Scopes = parameters.Scopes
            };
        
        _logger.LogDebug("Requesting client credentials access token");
        
        var response = await _openIddictClientService.AuthenticateWithClientCredentialsAsync(request).ConfigureAwait(false);

        if (response.TokenResponse.IsError())
        {
            return new ClientToken
            {
                Error = response.TokenResponse.Error
            };
        }
        
        return new ClientToken
        {
            AccessToken = response.AccessToken,
            AccessTokenType = response.TokenResponse.TokenType,
            Expiration = response.TokenResponse.ExpiresIn == 0
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.UtcNow.AddSeconds(response.TokenResponse.ExpiresIn!.Value),
            Scope = response.TokenResponse.Scope
        };
    }
}