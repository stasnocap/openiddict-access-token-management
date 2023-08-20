using System.Collections.Concurrent;
using OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;
using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// Default implementation for token request synchronization primitive
/// </summary>
internal class TokenRequestSynchronization : ITokenRequestSynchronization
{
    // this is what provides the synchronization; assumes this service is a singleton in DI.
    ConcurrentDictionary<string, Lazy<Task<ClientToken>>> _dictionary { get; } = new();

    /// <inheritdoc/>
    public async Task<ClientToken> SynchronizeAsync(string name, Func<Task<ClientToken>> func)
    {
        try
        {
            return await _dictionary.GetOrAdd(name, _ =>
            {
                return new Lazy<Task<ClientToken>>(func);
            }).Value.ConfigureAwait(false);
        }
        finally
        {
            _dictionary.TryRemove(name, out _);
        }
    }
}