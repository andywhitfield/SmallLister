using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetProfileRequest : IRequest<GetProfileResponse>
    {
        public ClaimsPrincipal User { get; }
        public GetProfileRequest(ClaimsPrincipal user) => User = user;
    }
}