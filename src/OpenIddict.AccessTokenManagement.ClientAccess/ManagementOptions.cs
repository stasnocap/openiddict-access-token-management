using OpenIddict.AccessTokenManagement.ClientAccess.Unauthorized;

namespace OpenIddict.AccessTokenManagement.ClientAccess;

/// <summary>
/// If you add something, add to ClientTokenManagementServiceCollectionExtensions.ConfigureManagementOptions
/// </summary>
public class ManagementOptions
{
    public string? EncryptionKey { get; set; }
    
    public bool UseUnauthorizedMiddleware { get; set; }

    public UnauthorizedRedirectOptions UnauthorizedRedirectOptions { get; set; } = new();
}