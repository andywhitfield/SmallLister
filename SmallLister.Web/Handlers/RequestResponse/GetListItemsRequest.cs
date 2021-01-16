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
        public int? PageNumber { get; }
        public GetListItemsRequest(ClaimsPrincipal user, string list, ItemSortOrder? sort, int? pageNumber)
        {
            User = user;
            List = list;
            Sort = sort;
            PageNumber = pageNumber;
        }
    }
}