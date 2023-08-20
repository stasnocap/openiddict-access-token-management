using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;

/// <summary>
/// Abstraction for managing client access tokens
/// </summary>
public interface IClientTokenManagementService
{
    /// <summary>
    /// Returns either a cached or a new access token for a given client configuration, the default client or a given token request
    /// </summary>
    /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
    /// <param name="parameters">Optional parameters.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <returns>The access token or null if the no token can be requested.</returns>
    Task<ClientToken> GetAccessTokenAsync(
        string clientName,
        TokenRequestParameters? parameters = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a client access token from the cache
    /// </summary>
    /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
    /// <param name="parameters">Optional parameters.</param>
    /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
    /// <returns>The access token or null if the no token can be requested.</returns>
    Task DeleteAccessTokenAsync(
        string clientName, 
        TokenRequestParameters? parameters = null, 
        CancellationToken cancellationToken = default);
}