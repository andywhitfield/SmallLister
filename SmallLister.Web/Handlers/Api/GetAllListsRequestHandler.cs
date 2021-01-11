using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse.Api;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.Api
{
    public class GetAllListsRequestHandler : IRequestHandler<GetAllListsRequest, GetAllListsResponse>
    {
        private readonly IUserListRepository _userListRepository;

        public GetAllListsRequestHandler(IUserListRepository userListRepository) => _userListRepository = userListRepository;

        public async Task<GetAllListsResponse> Handle(GetAllListsRequest request, CancellationToken cancellationToken)
        {
            var lists = await _userListRepository.GetListsAsync(request.User);
            return new GetAllListsResponse(lists.Select(l => new GetAllListsResponse.ListResponse(l.UserListId.ToString(), l.Name)));
        }
    }
}