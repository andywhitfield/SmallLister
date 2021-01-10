using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class UpdateExternalClientRequest : IRequest<bool>
    {
        public UpdateExternalClientRequest(ClaimsPrincipal user, int apiClientId, string state)
        {
            User = user;
            ApiClientId = apiClientId;
            State = state;
        }
        public ClaimsPrincipal User { get; }
        public int ApiClientId { get; }
        public string State { get; }
    }
}