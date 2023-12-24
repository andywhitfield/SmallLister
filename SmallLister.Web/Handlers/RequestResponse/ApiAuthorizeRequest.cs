using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class ApiAuthorizeRequest : IRequest<string?>
    {
        public ClaimsPrincipal User { get; }
        public AuthorizeRequest Model { get; }
        public ApiAuthorizeRequest(ClaimsPrincipal user, AuthorizeRequest model)
        {
            User = user;
            Model = model;
        }
    }
}