using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers.RequestResponse;

public class EditItemRequest(ClaimsPrincipal user, int userItemId, AddOrUpdateItemRequest model)
    : IRequest<(bool, string)>
{
    public ClaimsPrincipal User { get; } = user;
    public int UserItemId { get; } = userItemId;
    public AddOrUpdateItemRequest Model { get; } = model;
}