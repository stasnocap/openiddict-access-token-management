using OpenIddict.Abstractions;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Extensions;

public static class OpenIddictResponseExtensions
{
    public static bool IsError(this OpenIddictResponse response)
        => !string.IsNullOrWhiteSpace(response.Error);
}