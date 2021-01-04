using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class AddListRequest : IRequest
    {
        public ClaimsPrincipal User { get; }
        public AddOrUpdateListRequest Model { get; }
        public AddListRequest(ClaimsPrincipal user, AddOrUpdateListRequest model)
        {
            User = user;
            Model = model;
        }
    }
}