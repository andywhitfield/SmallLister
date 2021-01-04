using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class UpdateListRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserListId { get; }
        public AddOrUpdateListRequest Model { get; }
        public UpdateListRequest(ClaimsPrincipal user, int userListId, AddOrUpdateListRequest model)
        {
            User = user;
            UserListId = userListId;
            Model = model;
        }
    }
}