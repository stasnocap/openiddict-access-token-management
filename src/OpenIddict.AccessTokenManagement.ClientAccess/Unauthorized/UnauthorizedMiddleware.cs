using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace OpenIddict.AccessTokenManagement.ClientAccess.Unauthorized;

public class UnauthorizedMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IOptions<ManagementOptions> options)
    {
        var managementOptions = options.Value;

        try
        {
            await _next(httpContext);
        }
        catch (UnauthorizedException)
        {
            if (managementOptions.UnauthorizedRedirectOptions.RelativeUri is not null)
            {
                var notEncodedUri = managementOptions.UnauthorizedRedirectOptions.RelativeUri;

                if (managementOptions.UnauthorizedRedirectOptions.AppendReturnUrlToQuery)
                {
                    if (managementOptions.UnauthorizedRedirectOptions.AppendReturnUrlToQuery)
                    {
                        notEncodedUri = QueryHelpers.AddQueryString(notEncodedUri,
                            managementOptions.UnauthorizedRedirectOptions.ReturnUrlQueryName, httpContext.Request.Path);
                    }
                }

                var uri = UriHelper.Encode(new Uri(notEncodedUri, UriKind.Relative));

                httpContext.Response.Redirect(uri);
                return;
            }
        }
    }
}