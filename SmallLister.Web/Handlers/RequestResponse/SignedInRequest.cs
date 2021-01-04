using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class SignedInRequest : IRequest
    {
        public SignedInRequest(ClaimsPrincipal user) => User = user;

        public ClaimsPrincipal User { get; }
    }
}