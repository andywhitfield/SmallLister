using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetListItemsRequest : IRequest<GetListItemsResponse>
    {
        public ClaimsPrincipal User { get; }
        public string List { get; }
        public GetListItemsRequest(ClaimsPrincipal user, string list)
        {
            User = user;
            List = list;
        }
    }
}