namespace OpenIddict.AccessTokenManagement.ClientAccess.Unauthorized;

public class UnauthorizedRedirectOptions
{
    public string? RelativeUri { get; set; }
    public string ReturnUrlQueryName { get; set; } = "returnUrl";
    public bool AppendReturnUrlToQuery { get; set; }
}