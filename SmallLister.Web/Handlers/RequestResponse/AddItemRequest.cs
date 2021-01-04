using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class AddItemRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public AddOrUpdateItemRequest Model { get; }

        public AddItemRequest(ClaimsPrincipal user, AddOrUpdateItemRequest model)
        {
            User = user;
            Model = model;
        }
    }
}