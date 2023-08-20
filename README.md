# openiddict-access-token-management
Automatic token management for machine to machine and user-centric web app OAuth and OIDC flows

The project is highly inspired by [Duende.AccessTokenManagement](https://github.com/DuendeSoftware/Duende.AccessTokenManagement) (refactored to work with [OpenIddictClientService](https://github.com/openiddict/openiddict-core/blob/e3075f60b52329e2d2be37b060882a92b3b2e4f8/src/OpenIddict.Client/OpenIddictClientService.cs#L19))

## Client credentials

```xml
<ItemGroup>
    <PackageReference Include="stasnocap.OpenIddict.AccessTokenManagement.ClientAccess" Version="1.0.0" />
</ItemGroup>
```

### Setup

```csharp
services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow grant_type=client_credentials to be negotiated.
        options.AllowClientCredentialsFlow();

        // Disable token storage, which is not necessary for non-interactive flows like
        // grant_type=password, grant_type=client_credentials or grant_type=refresh_token.
        options.DisableTokenStorage();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration matching the client application definition in the server project.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44385/", UriKind.Absolute),

            ClientId = "console",
            ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207"
        });
    });

// default cache
services.AddDistributedMemoryCache();

services.AddClientAccessTokenManagement(options => {
    // or leave it empty and set options.DisableAccessTokenEncryption() on server
    options.EncryptionKey = "DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=";
});
```

### HTTP Client Factory

```csharp
services.AddClientAccessHttpClient("invoices", "invoice.client", client =>
{
    client.BaseAddress = new Uri("https://apis.company.com/invoice/");
});
```

```csharp
services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://apis.company.com/catalog/");
    })
    .AddClientAccessTokenHandler("catalog.client");
```

### Usage

#### Manual

```csharp
public class WorkerManual : BackgroundService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IClientTokenManagementService _tokenManagementService;

    public WorkerManualIHttpClientFactory factory, IClientTokenManagementService tokenManagementService)
    {
        _clientFactory = factory;
        _tokenManagementService = tokenManagementService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {          
        while (!stoppingToken.IsCancellationRequested)
        {
            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri("https://apis.company.com/catalog/");
            
            // get access token for client and set on HttpClient
            var token = await _tokenManagementService.GetAccessTokenAsync("catalog.client");
            client.SetBearerToken(token.Value);
            
            var response = await client.GetAsync("list", stoppingToken);
                
            // rest omitted
        }
    }
}
```

### HTTP factory

```csharp
public class WorkerHttpClient : BackgroundService
{
    private readonly ILogger<WorkerHttpClient> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public WorkerHttpClient(ILogger<WorkerHttpClient> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _clientFactory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var client = _clientFactory.CreateClient("invoices");
            var response = await client.GetAsync("test", stoppingToken);

            // rest omitted
        }
    }
}
```

## User

```xml
<ItemGroup>
    <PackageReference Include="stasnocap.OpenIddict.AccessTokenManagement.UserAccess" Version="1.0.0" />
</ItemGroup>
```

### Setup

```csharp
services.AddOpenIddict()

    // Register the OpenIddict client components.
    .AddClient(options =>
    {
        // Allow grant_type=refresh_token to be negotiated.
        options.AllowRefreshTokenFlow();

        // Disable token storage, which is not necessary for non-interactive flows like
        // grant_type=password, grant_type=client_credentials or grant_type=refresh_token.
        options.DisableTokenStorage();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
               .SetProductInformation(typeof(Program).Assembly);

        // Add a client registration without a client identifier/secret attached.
        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri("https://localhost:44382/", UriKind.Absolute)
        });
    });

// adds services for token management
builder.Services.AddUserAccessTokenManagement(options => {
    // or leave it empty and set options.DisableAccessTokenEncryption() on server
    options.EncryptionKey = "DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=";
});
```

### HTTP client factory

```csharp
// registers HTTP client that uses the managed user access token
builder.Services.AddUserAccessTokenHttpClient("invoices",
    configureClient: client => { client.BaseAddress = new Uri("https://api.company.com/invoices/"); });
```

```csharp
// registers a typed HTTP client with token management support
builder.Services.AddHttpClient<InvoiceClient>(client =>
    {
        client.BaseAddress = new Uri("https://api.company.com/invoices/");
    })
    .AddUserAccessTokenHandler();
```

## Usage

### Manually

```csharp
public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserTokenManagementService _tokenManagementService;

    public HomeController(IHttpClientFactory httpClientFactory, IUserTokenManagementService tokenManagementService)
    {
        _httpClientFactory = httpClientFactory;
        _tokenManagementService = tokenManagementService;
    }

    public async Task<IActionResult> CallApi()
    {
        var token = await _tokenManagementService.GetAccessTokenAsync(User);
        var client = _httpClientFactory.CreateClient();
        client.SetBearerToken(token.Value);
            
        var response = await client.GetAsync("https://api.company.com/invoices");
        
        // rest omitted
    }
}
```

### HttpContext extension method

```csharp
public async Task<IActionResult> CallApi()
{
    var token = await HttpContext.GetUserAccessTokenAsync();
    var client = _httpClientFactory.CreateClient();
    client.SetBearerToken(token.Value);
        
    var response = await client.GetAsync("https://api.company.com/invoices");
    
    // rest omitted
}
```

### HTTP client factory

```csharp
public async Task<IActionResult> CallApi()
{
    var client = _httpClientFactory.CreateClient("invoices");

    var response = await client.GetAsync("list");
    
    // rest omitted
}
```

```csharp
public async Task<IActionResult> CallApi([FromServices] InvoiceClient client)
{
    var response = await client.GetList();
    
    // rest omitted
}
```

## Refresh token and cookie on the server have expired

```csharp
services.AddUserAccessTokenManagement(options => {
    // or leave it empty and set options.DisableAccessTokenEncryption() on server
    options.UseUnauthorizedMiddleware = true;
    // redirect to login
    options.UnauthorizedRedirectOptions.RelativeUri = "/login";
    // append returnUrl to redirecti uri
    options.UnauthorizedRedirectOptions.AppendReturnUrlToQuery = true;
    // change returnUrl query name, default is 'returnUrl'
    options.UnauthorizedRedirectOptions.ReturnUrlQueryName = "myReturnUrlQueryName";
});
```

or same

```csharp
services.AddClientAccessTokenManagement(options => ...);
```
### Middleware

```csharp
app.UseMiddleware<UnauthorizedMiddleware>();
```
