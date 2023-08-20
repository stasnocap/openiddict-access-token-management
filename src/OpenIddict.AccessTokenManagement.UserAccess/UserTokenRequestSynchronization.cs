using System.Collections.Concurrent;
using OpenIddict.AccessTokenManagement.UserAccess.Interfaces;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess;

/// <summary>
/// Default implementation for token request synchronization primitive
/// </summary>
internal class UserTokenRequestSynchronization : IUserTokenRequestSynchronization
{
    // this is what provides the synchronization; assumes this service is a singleton in DI.
    ConcurrentDictionary<string, Lazy<Task<UserToken>>> _dictionary { get; } = new();

    /// <inheritdoc/>
    public async Task<UserToken> SynchronizeAsync(string name, Func<Task<UserToken>> func)
    {
        try
        {
            return await _dictionary.GetOrAdd(name, _ =>
            {
                return new Lazy<Task<UserToken>>(func);
            }).Value.ConfigureAwait(false);
        }
        finally
        {
            _dictionary.TryRemove(name, out _);
        }
    }
}