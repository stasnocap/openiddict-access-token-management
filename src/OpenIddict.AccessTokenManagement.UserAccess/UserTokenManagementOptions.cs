using OpenIddict.AccessTokenManagement.ClientAccess;

namespace OpenIddict.AccessTokenManagement.UserAccess;

/// <summary>
/// Options for user access token management
/// </summary>
public class UserTokenManagementOptions : ManagementOptions
{
    /// <summary>
    /// Boolean to set whether tokens added to a session should be challenge-scheme-specific.
    /// The default is 'false'.
    /// </summary>
    public bool UseChallengeSchemeScopedTokens { get; set; }

    /// <summary>
    /// Timespan that specifies how long before expiration, the token should be refreshed (defaults to 1 minute)
    /// </summary>
    public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
}