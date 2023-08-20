using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Delegating handler that injects a client credentials access token into an outgoing request
/// </summary>
public class ClientTokenHandler : AccessTokenHandler
{
    private readonly ClientTokenManagementOptions _clientTokenManagementOptions;
    private readonly IClientTokenManagementService _accessTokenManagementService;
    private readonly string _tokenClientName;

    public ClientTokenHandler(
        IOptions<ClientTokenManagementOptions> clientTokenManagementOptions, 
        IOptions<ManagementOptions> managementOptions,
        IClientTokenManagementService accessTokenManagementService,
        ILogger<ClientTokenHandler> logger,
        string tokenClientName) 
        : base(managementOptions, logger)
    {
        _clientTokenManagementOptions = clientTokenManagementOptions.Value;
        _accessTokenManagementService = accessTokenManagementService;
        _tokenClientName = tokenClientName;
    }

    /// <inheritdoc/>
    protected override Task<ClientToken> GetAccessTokenAsync(bool forceRenewal, CancellationToken cancellationToken)
    {
        var parameters = _clientTokenManagementOptions.DefaultTokenRequestParameters ?? new TokenRequestParameters
        {
            ForceRenewal = forceRenewal
        };
        
        return _accessTokenManagementService.GetAccessTokenAsync(_tokenClientName, parameters, cancellationToken);
    }
}