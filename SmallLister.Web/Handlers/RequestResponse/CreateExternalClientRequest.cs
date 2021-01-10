using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class CreateExternalClientRequest : IRequest<CreateExternalClientResponse>
    {
        public ClaimsPrincipal User { get; }
        public AddExternalClientRequest Model { get; }
        public CreateExternalClientRequest(ClaimsPrincipal user, AddExternalClientRequest model)
        {
            User = user;
            Model = model;
        }
    }
}