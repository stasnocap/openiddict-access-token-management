namespace OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

/// <summary>
/// Represents a client access token
/// </summary>
public class ClientToken
{
    /// <summary>
    /// The access token
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// The access token type
    /// </summary>
    public string? AccessTokenType { get; set; }
    
    /// <summary>
    /// The access token expiration
    /// </summary>
    public DateTimeOffset Expiration { get; set; }

    /// <summary>
    /// The scope of the access tokens
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Error (if any) during token request
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Checks for an error
    /// </summary>
    public bool IsError => !string.IsNullOrWhiteSpace(Error);
}