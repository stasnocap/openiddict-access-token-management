using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.AccessTokenManagement.ClientAccess;

namespace OpenIddict.AccessTokenManagement.UserAccess;

public static class JsonWebTokenService
{
    public static ManagementOptions GetTokenManagementOptionsFromHttpContext(HttpContext httpContext)
    {
        var userTokenManagementOptions = httpContext.RequestServices.GetRequiredService<IOptions<ManagementOptions>>();

        return userTokenManagementOptions.Value;
    }

    public static async Task<bool> CheckTokenExpirationAsync(string tokenName, HttpContext httpContext)
    {
        var token = await httpContext.GetTokenAsync(tokenName);

        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));
        
        if (!string.IsNullOrEmpty(token))
        {
            var managementOptions = GetTokenManagementOptionsFromHttpContext(httpContext);

            var expiration = GetExpiration(token, managementOptions.EncryptionKey);

            if (expiration > DateTimeOffset.UtcNow)
            {
                return true;
            }
        }

        return false;
    }

    public static DateTimeOffset GetExpiration(string token, string? encryptionKey)
    {
        var tokenHandler = new JsonWebTokenHandler();

        var jsonWebToken = tokenHandler.ReadJsonWebToken(token);

        if (!string.IsNullOrEmpty(encryptionKey))
        {
            var decryptedToken = tokenHandler.DecryptToken(jsonWebToken, new TokenValidationParameters()
            {
                TokenDecryptionKey = new SymmetricSecurityKey(Convert.FromBase64String(encryptionKey))
            });

            jsonWebToken = tokenHandler.ReadJsonWebToken(decryptedToken);
        }

        return jsonWebToken.ValidTo;
    }
}