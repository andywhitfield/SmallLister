using MediatR;
using SmallLister.Model;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.RequestResponse.Api
{
    public class GetListRequest : IRequest<GetListResponse>
    {
        public UserAccount User { get; }
        public string ListId { get; }
        public GetListRequest(UserAccount user, string listId)
        {
            User = user;
            ListId = listId;
        }
    }
}