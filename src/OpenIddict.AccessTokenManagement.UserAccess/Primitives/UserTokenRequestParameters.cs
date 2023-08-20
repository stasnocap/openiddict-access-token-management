using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Primitives;

/// <summary>
/// Additional optional per request parameters for a user access token request
/// </summary>
public class UserTokenRequestParameters : TokenRequestParameters
{
    /// <summary>
    /// Overrides the default sign-in scheme. This information may be used for state management.
    /// </summary>
    public string? SignInScheme { get; set; }
        
    /// <summary>
    /// Overrides the default challenge scheme. This information may be used for deriving token service configuration.
    /// </summary>
    public string? ChallengeScheme { get; set; }
}