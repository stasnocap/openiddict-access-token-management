using Microsoft.Extensions.Logging;
using OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Implements token management logic
/// </summary>
public class ClientTokenManagementService : IClientTokenManagementService
{
    private readonly ITokenRequestSynchronization _sync;
    private readonly IClientTokenEndpointService _clientTokenEndpointService;
    private readonly IClientTokenCache _tokenCache;
    private readonly ILogger<ClientTokenManagementService> _logger;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="sync"></param>
    /// <param name="clientTokenEndpointService"></param>
    /// <param name="tokenCache"></param>
    /// <param name="logger"></param>
    public ClientTokenManagementService(
        ITokenRequestSynchronization sync,
        IClientTokenEndpointService clientTokenEndpointService,
        IClientTokenCache tokenCache,
        ILogger<ClientTokenManagementService> logger)
    {
        _sync = sync;
        _clientTokenEndpointService = clientTokenEndpointService;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ClientToken> GetAccessTokenAsync(
        string clientName,
        TokenRequestParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TokenRequestParameters();

        if (parameters.ForceRenewal == false)
        {
            try
            {
                var item = await _tokenCache.GetAsync(clientName, parameters, cancellationToken).ConfigureAwait(false);
                if (item != null)
                {
                    return item;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error trying to obtain token from cache for client {clientName}. Error = {error}. Will obtain new token.", 
                    clientName, e.Message);
            }
        }

        return await _sync.SynchronizeAsync(clientName, async () =>
        {
            var token = await _clientTokenEndpointService.RequestToken(clientName, parameters, cancellationToken).ConfigureAwait(false);
            if (token.IsError)
            {
                _logger.LogError(
                    "Error requesting access token for client {clientName}. Error = {error}.",
                    clientName, token.Error);

                return token;
            }

            try
            {
                await _tokenCache.SetAsync(clientName, token, parameters, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error trying to set token in cache for client {clientName}. Error = {error}", 
                    clientName, e.Message);
            }

            return token;
        }).ConfigureAwait(false);
    }


    /// <inheritdoc/>
    public Task DeleteAccessTokenAsync(
        string clientName,
        TokenRequestParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        parameters ??= new TokenRequestParameters();
        return _tokenCache.DeleteAsync(clientName, parameters, cancellationToken);
    }
}