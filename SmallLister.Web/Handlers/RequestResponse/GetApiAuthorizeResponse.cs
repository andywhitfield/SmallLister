namespace SmallLister.Web.Handlers.RequestResponse;

public class GetApiAuthorizeResponse
{
    public static readonly GetApiAuthorizeResponse InvalidResponse = new GetApiAuthorizeResponse();
    public string AppKey { get; }
    public string RedirectUri { get; }
    public string ApplicationName { get; }
    public bool IsValid { get; }

    private GetApiAuthorizeResponse()
    {
        IsValid = false;
        AppKey = RedirectUri = ApplicationName = "";
    }

    public GetApiAuthorizeResponse(string appKey, string redirectUri, string applicationName)
    {
        IsValid =
            !string.IsNullOrEmpty(appKey) &&
            !string.IsNullOrEmpty(redirectUri) &&
            !string.IsNullOrEmpty(applicationName);

        AppKey = appKey;
        RedirectUri = redirectUri;
        ApplicationName = applicationName;
    }
}