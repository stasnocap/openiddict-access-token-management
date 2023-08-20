using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.ClientAccess;
using OpenIddict.AccessTokenManagement.ClientAccess.Extensions;
using OpenIddict.AccessTokenManagement.UserAccess.Interfaces;
using OpenIddict.AccessTokenManagement.UserAccess.Primitives;

namespace OpenIddict.AccessTokenManagement.UserAccess.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register the user token management services
/// </summary>
public static class UserTokenManagementServiceCollectionExtensions
{
    /// <summary>
    /// Adds the necessary services to manage user tokens based on OpenID Connect configuration
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddUserAccessTokenManagement(this IServiceCollection services,
        Action<UserTokenManagementOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);

            // ReSharper disable once SimplifyLinqExpressionUseAll
            if (!services.Any(x => x.ServiceType == typeof(IOptions<ManagementOptions>)))
            {
                ClientTokenManagementServiceCollectionExtensions.ConfigureManagementOptions(services, configureOptions);
            }
        }

        services.AddHttpContextAccessor();

        services.TryAddTransient<IUserTokenManagementService, UserAccessTokenManagementService>();
        services.TryAddScoped<IUserTokenStore, AuthenticationSessionUserAccessTokenStore>();
        services.TryAddSingleton<IUserTokenRequestSynchronization, UserTokenRequestSynchronization>();
        services.TryAddTransient<IUserTokenEndpointService, UserTokenEndpointService>();

        return services;
    }

    /// <summary>
    /// Adds a named HTTP client for the factory that automatically sends the current user access token
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The name of the client.</param>
    /// <param name="parameters"></param>
    /// <param name="configureClient">Additional configuration with service provider instance.</param>
    /// <returns></returns>
    public static IHttpClientBuilder AddUserAccessTokenHttpClient(this IServiceCollection services,
        string name,
        UserTokenRequestParameters? parameters = null,
        Action<IServiceProvider, HttpClient>? configureClient = null)
    {
        if (configureClient != null)
        {
            return services.AddHttpClient(name, configureClient)
                .AddUserAccessTokenHandler(parameters);
        }

        return services.AddHttpClient(name)
            .AddUserAccessTokenHandler(parameters);
    }

    /// <summary>
    /// Adds the user access token handler to an HttpClient
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddUserAccessTokenHandler(
        this IHttpClientBuilder httpClientBuilder,
        UserTokenRequestParameters? parameters = null)
    {
        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var logger = provider.GetRequiredService<ILogger<OpenIddictUserAccessTokenHandler>>();
            var managementOptions = provider.GetRequiredService<IOptions<ManagementOptions>>();

            return new OpenIddictUserAccessTokenHandler(managementOptions, contextAccessor, logger, parameters);
        });
    }
}