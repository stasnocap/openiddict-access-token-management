using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Client access token cache using IDistributedCache
/// </summary>
public class DistributedClientTokenCache : IClientTokenCache
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedClientTokenCache> _logger;
    private readonly ClientTokenManagementOptions _options;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public DistributedClientTokenCache(
        IDistributedCache cache, 
        IOptions<ClientTokenManagementOptions> options, 
        ILogger<DistributedClientTokenCache> logger)
    {
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }
        
    /// <inheritdoc/>
    public async Task SetAsync(
        string clientName,
        ClientToken clientToken,
        TokenRequestParameters requestParameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);
            
        var cacheExpiration = clientToken.Expiration.AddSeconds(-_options.CacheLifetimeBuffer);
        var data = JsonSerializer.Serialize(clientToken);

        var entryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = cacheExpiration
        };

        _logger.LogTrace("Caching access token for client: {clientName}. Expiration: {expiration}", clientName, cacheExpiration);
            
        var cacheKey = GenerateCacheKey(_options, clientName, requestParameters);
        await _cache.SetStringAsync(cacheKey, data, entryOptions, token: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ClientToken?> GetAsync(
        string clientName, 
        TokenRequestParameters requestParameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);
            
        var cacheKey = GenerateCacheKey(_options, clientName, requestParameters);
        var entry = await _cache.GetStringAsync(cacheKey, token: cancellationToken).ConfigureAwait(false);

        if (entry != null)
        {
            try
            {
                _logger.LogDebug("Cache hit for access token for client: {clientName}", clientName);
                return JsonSerializer.Deserialize<ClientToken>(entry);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error parsing cached access token for client {clientName}", clientName);
                return null;
            }
        }

        _logger.LogTrace("Cache miss for access token for client: {clientName}", clientName);
        return null;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(
        string clientName,
        TokenRequestParameters requestParameters,
        CancellationToken cancellationToken = default)
    {
        if (clientName is null) throw new ArgumentNullException(nameof(clientName));

        var cacheKey = GenerateCacheKey(_options, clientName, requestParameters);
        return _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    /// <summary>
    /// Generates the cache key based on various inputs
    /// </summary>
    /// <param name="options"></param>
    /// <param name="clientName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    protected virtual string GenerateCacheKey(
        ClientTokenManagementOptions options, 
        string clientName,
        TokenRequestParameters? parameters = null)
    {
        var s = "s_" + parameters?.Scopes ?? "";

        return options.CacheKeyPrefix + clientName + "::" + s ;
    }
}