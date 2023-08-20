using OpenIddict.Abstractions;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Primitives;

/// <summary>
/// Additional optional parameters for a client credentials access token request
/// </summary>
public class TokenRequestParameters
{
    /// <summary>
    /// Force renewal of token.
    /// </summary>
    public bool ForceRenewal { get; set; }

    /// <summary>
    /// Gets or sets the parameters that will be added to the token request.
    /// </summary>
    public Dictionary<string, OpenIddictParameter>? AdditionalTokenRequestParameters { get; init; }

    /// <summary>
    /// Gets or sets the application-specific properties that will be added to the context.
    /// </summary>
    public Dictionary<string, string?>? Properties { get; init; }

    /// <summary>
    /// Gets or sets the provider name used to resolve the client registration.
    /// </summary>
    /// <remarks>
    /// Note: if multiple client registrations use the same provider name.
    /// the <see cref="RegistrationId"/> property must be explicitly set.
    /// </remarks>
    public string? ProviderName { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the client registration that will be used.
    /// </summary>
    public string? RegistrationId { get; init; }

    /// <summary>
    /// Gets the scopes that will be sent to the authorization server.
    /// </summary>
    public List<string>? Scopes { get; init; }

    /// <summary>
    /// Gets or sets the issuer used to resolve the client registration.
    /// </summary>
    /// <remarks>
    /// Note: if multiple client registrations point to the same issuer,
    /// the <see cref="RegistrationId"/> property must be explicitly set.
    /// </remarks>
    public Uri? Issuer { get; init; }
}