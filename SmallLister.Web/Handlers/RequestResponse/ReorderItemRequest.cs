using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class ReorderItemRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserItemId { get; }
        public MoveRequest Model { get; }
        public ReorderItemRequest(ClaimsPrincipal user, int userItemId, MoveRequest model)
        {
            User = user;
            UserItemId = userItemId;
            Model = model;
        }
    }
}