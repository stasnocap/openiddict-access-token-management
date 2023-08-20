using OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Primitives;

/// <summary>
/// Models a user access token
/// </summary>
public class UserToken : ClientToken
{
    /// <summary>
    /// The refresh token
    /// </summary>
    public string? RefreshToken { get; set; }
}