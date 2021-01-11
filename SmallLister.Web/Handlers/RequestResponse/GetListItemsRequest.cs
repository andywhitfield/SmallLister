using System.Security.Claims;
using MediatR;
using SmallLister.Model;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetListItemsRequest : IRequest<GetListItemsResponse>
    {
        public ClaimsPrincipal User { get; }
        public string List { get; }
        public ItemSortOrder? Sort { get; }
        public GetListItemsRequest(ClaimsPrincipal user, string list, ItemSortOrder? sort)
        {
            User = user;
            List = list;
            Sort = sort;
        }
    }
}