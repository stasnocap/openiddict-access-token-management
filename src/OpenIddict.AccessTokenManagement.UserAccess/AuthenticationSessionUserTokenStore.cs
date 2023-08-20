﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.UserAccess.Interfaces;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess
{
    /// <summary>
    /// Token store using the ASP.NET Core authentication session
    /// </summary>
    public class AuthenticationSessionUserAccessTokenStore : IUserTokenStore
    {
        private const string TokenPrefix = ".Token.";
        private const string TokenNamesKey = ".TokenNames";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<AuthenticationSessionUserAccessTokenStore> _logger;
        private readonly UserTokenManagementOptions _options;

        // per-request cache so that if SignInAsync is used, we won't re-read the old/cached AuthenticateResult from the handler
        // this requires this service to be added as scoped to the DI system
        private readonly Dictionary<string, AuthenticateResult> _cache = new();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="logger"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AuthenticationSessionUserAccessTokenStore(
            IHttpContextAccessor contextAccessor,
            ILogger<AuthenticationSessionUserAccessTokenStore> logger,
            IOptions<UserTokenManagementOptions> options)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<UserToken> GetTokenAsync(
            ClaimsPrincipal user,
            UserTokenRequestParameters? parameters = null)
        {
            parameters ??= new();

            // check the cache in case the cookie was re-issued via StoreTokenAsync
            // we use String.Empty as the key for a null SignInScheme
            if (!_cache.TryGetValue(parameters.SignInScheme ?? String.Empty, out var result))
            {
                result = await _contextAccessor!.HttpContext!.AuthenticateAsync(parameters.SignInScheme).ConfigureAwait(false);
            }

            if (!result.Succeeded)
            {
                _logger.LogInformation("Cannot authenticate scheme: {scheme}", parameters.SignInScheme ?? "default signin scheme");

                return new UserToken() { Error = "Cannot authenticate scheme" };
            }

            if (result.Properties == null)
            {
                _logger.LogInformation("Authentication result properties are null for scheme: {scheme}",
                    parameters.SignInScheme ?? "default signin scheme");

                return new UserToken() { Error = "No properties on authentication result" };
            }

            return GetTokenFromProperties(result.Properties, parameters);
        }

        /// <inheritdoc/>
        public UserToken GetTokenFromProperties(AuthenticationProperties authenticationProperties, UserTokenRequestParameters? parameters = null)
        {
            var tokens = authenticationProperties.Items.Where(i => i.Key.StartsWith(TokenPrefix)).ToList();
            if (!tokens.Any())
            {
                _logger.LogInformation("No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.");

                return new UserToken() { Error = "No tokens in properties" };
            }

            const string tokenName = $"{TokenPrefix}{UserTokenDefaults.AccessTokenName}";
            const string tokenTypeName = $"{TokenPrefix}{UserTokenDefaults.AccessTokenTypeName}";
            const string refreshTokenName = $"{TokenPrefix}{UserTokenDefaults.RefreshTokenName}";

            string? refreshToken = null;
            string? accessToken = null;
            string? accessTokenType = null;

            if (AppendChallengeSchemeToTokenNames(parameters))
            {
                refreshToken = tokens
                        .SingleOrDefault(t => t.Key == $"{refreshTokenName}||{parameters.ChallengeScheme}").Value;
                accessToken = tokens.SingleOrDefault(t => t.Key == $"{tokenName}||{parameters.ChallengeScheme}")
                    .Value;
                accessTokenType = tokens.SingleOrDefault(t => t.Key == $"{tokenTypeName}||{parameters.ChallengeScheme}")
                    .Value;
            }

            refreshToken ??= tokens.SingleOrDefault(t => t.Key == $"{refreshTokenName}").Value;
            accessToken ??= tokens.SingleOrDefault(t => t.Key == $"{tokenName}").Value;
            accessTokenType ??= tokens.SingleOrDefault(t => t.Key == $"{tokenTypeName}").Value;

            DateTimeOffset dtExpires = DateTimeOffset.MaxValue;
            if (!string.IsNullOrEmpty(accessToken))
            {
                dtExpires = JsonWebTokenService.GetExpiration(accessToken, _options.EncryptionKey);
            }
            
            return new UserToken
            {
                AccessToken = accessToken,
                AccessTokenType = accessTokenType,
                RefreshToken = refreshToken,
                Expiration = dtExpires
            };
        }

        /// <inheritdoc/>
        public async Task StoreTokenAsync(
            ClaimsPrincipal user,
            UserToken token,
            UserTokenRequestParameters? parameters = null)
        {
            parameters ??= new();

            // check the cache in case the cookie was re-issued via StoreTokenAsync
            // we use String.Empty as the key for a null SignInScheme
            if (!_cache.TryGetValue(parameters.SignInScheme ?? String.Empty, out var result))
            {
                result = await _contextAccessor!.HttpContext!.AuthenticateAsync(parameters.SignInScheme)!.ConfigureAwait(false);
            }

            if (result is not { Succeeded: true })
            {
                throw new Exception("Can't store tokens. User is anonymous");
            }

            // in case you want to filter certain claims before re-issuing the authentication session
            var transformedPrincipal = await FilterPrincipalAsync(result.Principal!).ConfigureAwait(false);

            var tokenName = $"{TokenPrefix}{UserTokenDefaults.AccessTokenName}";
            var tokenTypeName = $"{TokenPrefix}{UserTokenDefaults.AccessTokenTypeName}";
            var refreshTokenName = $"{UserTokenDefaults.RefreshTokenName}";

            if (AppendChallengeSchemeToTokenNames(parameters))
            {
                refreshTokenName += $"||{parameters.ChallengeScheme}";
                tokenName += $"||{parameters.ChallengeScheme}";
                tokenTypeName += $"||{parameters.ChallengeScheme}";
            }

            result.Properties!.Items[tokenName] = token.AccessToken;
            result.Properties!.Items[tokenTypeName] = token.AccessTokenType;

            if (token.RefreshToken != null)
            {
                if (!result.Properties.UpdateTokenValue(refreshTokenName, token.RefreshToken))
                {
                    result.Properties.Items[$"{TokenPrefix}{refreshTokenName}"] = token.RefreshToken;
                }
            }

            var options = _contextAccessor!.HttpContext!.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
            var schemeProvider = _contextAccessor.HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = parameters.SignInScheme ?? (await schemeProvider.GetDefaultSignInSchemeAsync().ConfigureAwait(false))?.Name;
            var cookieOptions = options.Get(scheme);

            if (result.Properties.AllowRefresh == true ||
                (result.Properties.AllowRefresh == null && cookieOptions.SlidingExpiration))
            {
                // this will allow the cookie to be issued with a new issued (and thus a new expiration)
                result.Properties.IssuedUtc = null;
                result.Properties.ExpiresUtc = null;
            }

            result.Properties.Items.Remove(TokenNamesKey);
            result.Properties.Items.Add(new KeyValuePair<string, string?>(TokenNamesKey, string.Join(";", result.Properties.Items.Select(t => t.Key).ToList())));

            await _contextAccessor.HttpContext.SignInAsync(parameters.SignInScheme, transformedPrincipal, result.Properties).ConfigureAwait(false);

            // add to the cache so if GetTokenAsync is called again, we will use the updated property values
            // we use String.Empty as the key for a null SignInScheme
            _cache[parameters.SignInScheme ?? String.Empty] = AuthenticateResult.Success(new AuthenticationTicket(transformedPrincipal, result.Properties, scheme!));
        }

        /// <inheritdoc/>
        public Task ClearTokenAsync(
            ClaimsPrincipal user, 
            UserTokenRequestParameters? parameters = null)
        {
            // don't bother here, since likely we're in the middle of signing out
            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows transforming the principal before re-issuing the authentication session
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        protected virtual Task<ClaimsPrincipal> FilterPrincipalAsync(ClaimsPrincipal principal)
        {
            return Task.FromResult(principal);
        }

        /// <summary>
        /// Confirm application has opted in to UseChallengeSchemeScopedTokens and a ChallengeScheme is provided upon storage and retrieval of tokens.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual bool AppendChallengeSchemeToTokenNames(UserTokenRequestParameters parameters)
        {
            return _options.UseChallengeSchemeScopedTokens && !string.IsNullOrEmpty(parameters!.ChallengeScheme);
        }
    }
}