using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class EditItemRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int UserItemId { get; }
        public AddOrUpdateItemRequest Model { get; }
        public EditItemRequest(ClaimsPrincipal user, int userItemId, AddOrUpdateItemRequest model)
        {
            User = user;
            UserItemId = userItemId;
            Model = model;
        }
    }
}