using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers
{
    public class GetListsRequestHandler : IRequestHandler<GetListsRequest, IEnumerable<UserListModel>>
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        public GetListsRequestHandler(IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }

        public async Task<IEnumerable<UserListModel>> Handle(GetListsRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var lists = await _userListRepository.GetListsAsync(user);
            return lists.Select(l => new UserListModel
            {
                UserListId = l.UserListId.ToString(),
                Name = l.Name
            });
        }
    }
}