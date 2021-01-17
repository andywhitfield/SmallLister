using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class CreateFeedRequest : IRequest
    {
        public CreateFeedRequest(ClaimsPrincipal user, AddFeedRequest model)
        {
            User = user;
            Model = model;
        }
        public ClaimsPrincipal User { get; }
        public AddFeedRequest Model { get; }
    }
}