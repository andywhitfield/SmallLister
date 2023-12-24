using System.Security.Claims;
using MediatR;
using SmallLister.Model;

namespace SmallLister.Web.Handlers.RequestResponse;

public class GetListItemsRequest(ClaimsPrincipal user, string? list, ItemSortOrder? sort, int? pageNumber)
    : IRequest<GetListItemsResponse>
{
    public ClaimsPrincipal User { get; } = user;
    public string? List { get; } = list;
    public ItemSortOrder? Sort { get; } = sort;
    public int? PageNumber { get; } = pageNumber;
}