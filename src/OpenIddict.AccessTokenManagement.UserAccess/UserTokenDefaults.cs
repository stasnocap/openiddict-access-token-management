using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenIddict.Client.AspNetCore;

namespace OpenIddict.AccessTokenManagement.UserAccess;

public static class UserTokenDefaults
{
    public const string AccessTokenName = OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken;
    
    public const string AccessTokenTypeName = OpenIdConnectParameterNames.TokenType;

    public const string RefreshTokenName = OpenIdConnectParameterNames.RefreshToken;
}