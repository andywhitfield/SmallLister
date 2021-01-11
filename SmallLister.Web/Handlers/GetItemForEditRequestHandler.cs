using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Handlers
{
    public class GetItemForEditRequestHandler : IRequestHandler<GetItemForEditRequest, GetItemForEditResponse>
    {
        private readonly ILogger<GetItemForEditRequestHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public GetItemForEditRequestHandler(ILogger<GetItemForEditRequestHandler> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
            IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<GetItemForEditResponse> Handle(GetItemForEditRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var item = await _userItemRepository.GetItemAsync(user, request.UserItemId);
            if (item == null)
            {
                _logger.LogInformation($"Could not item {request.UserItemId}");
                return GetItemForEditResponse.InvalidResponse;
            }

            var userLists = await _userListRepository.GetListsAsync(user);
            var lists = userLists
                .Select(l => new UserListModel { UserListId = l.UserListId.ToString(), Name = l.Name, CanAddItems = true })
                .Prepend(new UserListModel { Name = "All", UserListId = IndexViewModel.AllList, CanAddItems = true });
            var selectedList = lists.FirstOrDefault(l => l.UserListId == item.UserListId?.ToString());
            if (selectedList == null)
                selectedList = lists.Single(l => l.UserListId == IndexViewModel.AllList);

            return new GetItemForEditResponse(new UserItemModel(item), lists, selectedList);
        }
    }
}