namespace SmallLister.Web.Model.Response;

public class GetAccessTokenResponse(string? errorCode, string? accessToken)
{
    public string? ErrorCode { get; } = errorCode;
    public string? AccessToken { get; } = accessToken;
}