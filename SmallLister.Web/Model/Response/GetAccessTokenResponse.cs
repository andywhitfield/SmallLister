namespace SmallLister.Web.Model.Response
{
    public class GetAccessTokenResponse
    {
        public GetAccessTokenResponse(string errorCode, string accessToken)
        {
            ErrorCode = errorCode;
            AccessToken = accessToken;
        }
        public string ErrorCode { get; }
        public string AccessToken { get; }
    }
}