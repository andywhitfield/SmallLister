using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class SnoozeRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserItemId { get; }
        public SnoozeRequest(ClaimsPrincipal user, int userItemId)
        {
            User = user;
            UserItemId = userItemId;
        }
    }
}