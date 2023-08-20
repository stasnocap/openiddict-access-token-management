using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.UserAccess.Interfaces;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;
using OpenIddict.Client;

namespace OpenIddict.AccessTokenManagement.UserAccess;

/// <summary>
/// Implements token endpoint operations using IdentityModel
/// </summary>
public class UserTokenEndpointService : IUserTokenEndpointService
{
    private readonly OpenIddictClientService _openIddictClientService;
    private readonly ILogger<UserTokenEndpointService> _logger;
    private readonly UserTokenManagementOptions _options;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="openIddictClientService"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public UserTokenEndpointService(
        OpenIddictClientService openIddictClientService,
        IOptions<UserTokenManagementOptions> options,
        ILogger<UserTokenEndpointService> logger)
    {
        _openIddictClientService = openIddictClientService;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<UserToken> RefreshAccessTokenAsync(
        UserToken userToken,
        UserTokenRequestParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = userToken.RefreshToken ?? throw new ArgumentNullException(nameof(userToken.RefreshToken));

        _logger.LogTrace("Refreshing refresh token: {token}",  refreshToken);

        OpenIddictClientModels.RefreshTokenAuthenticationResult? result;
        var token = new UserToken();
        try
        {
            result = await _openIddictClientService.AuthenticateWithRefreshTokenAsync(
                new OpenIddictClientModels.RefreshTokenAuthenticationRequest
                {
                    RefreshToken = userToken.RefreshToken,
                    CancellationToken = cancellationToken,
                    Properties = parameters.Properties,
                    ProviderName = parameters.ProviderName,
                    Issuer = parameters.Issuer,
                    Scopes = parameters.Scopes,
                    RegistrationId = parameters.RegistrationId,
                    AdditionalTokenRequestParameters = parameters.AdditionalTokenRequestParameters
                });
        }
        catch (Exception exception)
        {
            token.Error = exception.Message;
            return token;
        }
        
        token.AccessToken = result.AccessToken;
        token.AccessTokenType = result.TokenResponse.TokenType;
        token.Expiration = result.TokenResponse.ExpiresIn == 0
            ? DateTimeOffset.MaxValue
            : DateTimeOffset.UtcNow.AddSeconds(result.TokenResponse.ExpiresIn!.Value);
        token.RefreshToken = result.RefreshToken;
        token.Scope = result.TokenResponse.Scope;    
        
        return token;
    }
}