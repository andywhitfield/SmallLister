using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse;

public class GetActionLogRequest(ClaimsPrincipal user) : IRequest<GetActionLogResponse>
{
    public ClaimsPrincipal User { get; } = user;
}
