using System.Collections.Generic;
using System.Security.Claims;
using MediatR;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetListsRequest : IRequest<IEnumerable<UserListModel>>
    {
        public ClaimsPrincipal User { get; }
        public GetListsRequest(ClaimsPrincipal user) => User = user;
    }
}