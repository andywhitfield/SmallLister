using MediatR;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class ValidateCredentialsAndCreateAccessTokenRequest : IRequest<GetAccessTokenResponse?>
    {
        public ValidateCredentialsAndCreateAccessTokenRequest(string credentials, string refreshToken)
        {
            Credentials = credentials;
            RefreshToken = refreshToken;
        }
        public string Credentials { get; }
        public string RefreshToken { get; }
    }
}