using MediatR;
using SmallLister.Model;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.RequestResponse.Api
{
    public class GetAllListsRequest : IRequest<GetAllListsResponse>
    {
        public UserAccount User { get; }
        public GetAllListsRequest(UserAccount user) => User = user;
    }
}