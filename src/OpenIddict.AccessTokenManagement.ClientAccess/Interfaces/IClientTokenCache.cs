using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;

/// <summary>
/// Abstraction for caching client credentials access tokens
/// </summary>
public interface IClientTokenCache
{
    /// <summary>
    /// Caches a client access token
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="clientToken"></param>
    /// <param name="requestParameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetAsync(
        string clientName,
        ClientToken clientToken,
        TokenRequestParameters requestParameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a client access token from the cache
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="requestParameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ClientToken?> GetAsync(
        string clientName,
        TokenRequestParameters requestParameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a client access token from the cache
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="requestParameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteAsync(
        string clientName,
        TokenRequestParameters requestParameters,
        CancellationToken cancellationToken = default);
}