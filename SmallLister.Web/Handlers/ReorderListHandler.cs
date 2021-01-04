using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class ReorderListHandler : IRequestHandler<ReorderListRequest, bool>
    {
        private readonly ILogger<ReorderListHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;

        public ReorderListHandler(ILogger<ReorderListHandler> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }

        public async Task<bool> Handle(ReorderListRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var list = await _userListRepository.GetListAsync(user, request.UserListId);
            if (list == null)
            {
                _logger.LogInformation($"Could not find list {request.UserListId}");
                return false;
            }

            UserList precedingList = null;
            if (request.Model.SortOrderPreviousListItemId != null)
            {
                precedingList = await _userListRepository.GetListAsync(user, request.Model.SortOrderPreviousListItemId.Value);
                if (precedingList == null)
                {
                    _logger.LogInformation($"Could not find preceding list {request.Model.SortOrderPreviousListItemId}");
                    return false;
                }
            }

            _logger.LogInformation($"Updating order of list {list.UserListId} [{list.Name}] to come after list {precedingList?.UserListId} [{precedingList?.Name}]");
            await _userListRepository.UpdateOrderAsync(list, precedingList);
            return true;
        }
    }
}