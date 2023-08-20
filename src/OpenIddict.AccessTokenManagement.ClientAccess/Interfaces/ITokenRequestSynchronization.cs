using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;

/// <summary>
/// Service to provide synchronization to token endpoint requests
/// </summary>
public interface ITokenRequestSynchronization
{
    /// <summary>
    /// Method to perform synchronization of work.
    /// </summary>
    public Task<ClientToken> SynchronizeAsync(string name, Func<Task<ClientToken>> func);
}