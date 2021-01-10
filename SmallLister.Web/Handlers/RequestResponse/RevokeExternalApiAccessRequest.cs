using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class RevokeExternalApiAccessRequest : IRequest<bool>
    {
        public RevokeExternalApiAccessRequest(ClaimsPrincipal user, int userAccountApiAccessId)
        {
            User = user;
            UserAccountApiAccessId = userAccountApiAccessId;
        }
        public ClaimsPrincipal User { get; }
        public int UserAccountApiAccessId { get; }
    }
}