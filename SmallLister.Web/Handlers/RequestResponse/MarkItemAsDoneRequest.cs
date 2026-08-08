using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse;

public class MarkItemAsDoneRequest(ClaimsPrincipal user, int userItemId)
    : IRequest<(bool, string)>
{
    public ClaimsPrincipal User { get; } = user;
    public int UserItemId { get; } = userItemId;
}