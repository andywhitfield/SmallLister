using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class MarkItemAsDoneRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserItemId { get; }
        public MarkItemAsDoneRequest(ClaimsPrincipal user, int userItemId)
        {
            User = user;
            UserItemId = userItemId;
        }
    }
}