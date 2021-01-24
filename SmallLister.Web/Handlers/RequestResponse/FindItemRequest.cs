using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class FindItemRequest : IRequest<FindItemResponse>
    {
        public FindItemRequest(ClaimsPrincipal user, string searchQuery)
        {
            User = user;
            SearchQuery = searchQuery;
        }
        public ClaimsPrincipal User { get; }
        public string SearchQuery { get; }
    }
}