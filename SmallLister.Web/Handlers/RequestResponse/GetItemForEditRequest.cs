using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetItemForEditRequest : IRequest<GetItemForEditResponse>
    {
        public ClaimsPrincipal User { get; }
        public int UserItemId { get; }
        public GetItemForEditRequest(ClaimsPrincipal user, int userItemId)
        {
            User = user;
            UserItemId = userItemId;
        }
    }
}