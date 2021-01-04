using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class ReorderListRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserListId { get; }
        public MoveRequest Model { get; }
        public ReorderListRequest(ClaimsPrincipal user, int userListId, MoveRequest model)
        {
            User = user;
            UserListId = userListId;
            Model = model;
        }
    }
}