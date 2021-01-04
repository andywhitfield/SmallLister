using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class DeleteListRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserListId { get; }
        public DeleteListRequest(ClaimsPrincipal user, int userListId)
        {
            User = user;
            UserListId = userListId;
        }
    }
}