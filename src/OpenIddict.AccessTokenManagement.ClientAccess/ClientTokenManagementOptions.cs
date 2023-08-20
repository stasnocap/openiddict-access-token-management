using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Client access token options
/// </summary>
public class ClientTokenManagementOptions : ManagementOptions
{
    /// <summary>
    /// Used to prefix the cache key
    /// </summary>
    public string CacheKeyPrefix { get; set; } = "OpenIddict.AccessTokenManagement.Cache::";

    /// <summary>
    /// Value to subtract from token lifetime for the cache entry lifetime (defaults to 60 seconds)
    /// </summary>
    public int CacheLifetimeBuffer { get; set; } = 60;
    
    public TokenRequestParameters? DefaultTokenRequestParameters { get; set; }
}