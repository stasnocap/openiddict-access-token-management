using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Interfaces;

/// <summary>
/// Service to provide synchronization to token endpoint requests
/// </summary>
public interface IUserTokenRequestSynchronization
{
    /// <summary>
    /// Method to perform synchronization of work.
    /// </summary>
    public Task<UserToken> SynchronizeAsync(string name, Func<Task<UserToken>> func);
}