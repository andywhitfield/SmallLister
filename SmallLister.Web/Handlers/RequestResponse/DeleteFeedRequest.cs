using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class DeleteFeedRequest : IRequest<bool>
    {
        public DeleteFeedRequest(ClaimsPrincipal user, int userFeedId)
        {
            User = user;
            UserFeedId = userFeedId;
        }
        public ClaimsPrincipal User { get; }
        public int UserFeedId { get; }
    }
}