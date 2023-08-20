using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.AccessTokenManagement.ClientAccess.Interfaces;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register the client credentials token management services
/// </summary>
public static class ClientTokenManagementServiceCollectionExtensions
{
    /// <summary>
    /// Adds all necessary services for client credentials token management
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddClientAccessTokenManagement(
        this IServiceCollection services,
        Action<ClientTokenManagementOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
            
            // ReSharper disable once SimplifyLinqExpressionUseAll
            if (!services.Any(x => x.ServiceType == typeof(IOptions<ManagementOptions>)))
            {
                ConfigureManagementOptions(services, configureOptions);
            }
        }

        services.TryAddSingleton<ITokenRequestSynchronization, TokenRequestSynchronization>();

        services.TryAddTransient<IClientTokenManagementService, ClientTokenManagementService>();
        services.TryAddTransient<IClientTokenCache, DistributedClientTokenCache>();
        services.TryAddTransient<IClientTokenEndpointService, ClientTokenEndpointService>();

        return services;
    }
    
    // TODO: refactor
    public static void ConfigureManagementOptions<T>(IServiceCollection services,
        Action<T> configureOptions) where T : ManagementOptions, new()
    {
        var tokenManagementOptions = new T();

        configureOptions(tokenManagementOptions);

        services.Configure<ManagementOptions>(x =>
        {
            x.EncryptionKey = tokenManagementOptions.EncryptionKey;
            x.UseUnauthorizedMiddleware = tokenManagementOptions.UseUnauthorizedMiddleware;
            x.UnauthorizedRedirectOptions.RelativeUri = tokenManagementOptions.UnauthorizedRedirectOptions.RelativeUri;
            x.UnauthorizedRedirectOptions.AppendReturnUrlToQuery = tokenManagementOptions.UnauthorizedRedirectOptions.AppendReturnUrlToQuery;
        });
    }

    /// <summary>
    /// Adds a named HTTP client for the factory that automatically sends the a client access token
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="httpClientName">The name of the client.</param>
    /// <param name="tokenClientName">The name of the token client.</param>
    /// <param name="configureClient">Additional configuration with service provider instance.</param>
    /// <returns></returns>
    public static IHttpClientBuilder AddClientAccessHttpClient(
        this IServiceCollection services,
        string httpClientName,
        Action<IServiceProvider, HttpClient>? configureClient = null)
    {
        ArgumentNullException.ThrowIfNull(httpClientName);

        if (configureClient != null)
        {
            return services.AddHttpClient(httpClientName, configureClient)
                .AddClientAccessTokenHandler(httpClientName);
        }

        return services.AddHttpClient(httpClientName)
            .AddClientAccessTokenHandler(httpClientName);
    }

    /// <summary>
    /// Adds the client access token handler to an HttpClient
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="tokenClientName"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddClientAccessTokenHandler(
        this IHttpClientBuilder httpClientBuilder,
        string tokenClientName)
    {
        ArgumentNullException.ThrowIfNull(tokenClientName);

        return httpClientBuilder.AddHttpMessageHandler(provider =>
        {
            var accessTokenManagementService = provider.GetRequiredService<IClientTokenManagementService>();
            var logger = provider.GetRequiredService<ILogger<ClientTokenHandler>>();
            var managementOptions = provider.GetRequiredService<IOptions<ManagementOptions>>();
            var clientTokenManagementOptions = provider.GetRequiredService<IOptions<ClientTokenManagementOptions>>();

            return new ClientTokenHandler(clientTokenManagementOptions, managementOptions, accessTokenManagementService, logger,
                tokenClientName);
        });
    }
}