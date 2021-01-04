using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class AddOrUpdateUserItemRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public AddOrUpdateItemRequest Request { get; }

        public AddOrUpdateUserItemRequest(ClaimsPrincipal user, AddOrUpdateItemRequest request)
        {
            User = user;
            Request = request;
        }
    }
}