using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class ReorderItemHandler : IRequestHandler<ReorderItemRequest, bool>
    {
        private readonly ILogger<ReorderItemHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserItemRepository _userItemRepository;

        public ReorderItemHandler(ILogger<ReorderItemHandler> logger,
            IUserAccountRepository userAccountRepository, IUserItemRepository userItemRepository,
            IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<bool> Handle(ReorderItemRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var item = await _userItemRepository.GetItemAsync(user, request.UserItemId);
            if (item == null)
            {
                _logger.LogInformation($"Could not find item {request.UserItemId}");
                return false;
            }
            if (item.UserListId == null)
            {
                _logger.LogInformation($"Item {request.UserItemId} has no list - can only order items on a specific list not the implicit 'all' list");
                return false;
            }

            UserItem precedingItem = null;
            if (request.Model.SortOrderPreviousListItemId != null)
            {
                precedingItem = await _userItemRepository.GetItemAsync(user, request.Model.SortOrderPreviousListItemId.Value);
                if (precedingItem == null)
                {
                    _logger.LogInformation($"Could not find preceding item {request.Model.SortOrderPreviousListItemId}");
                    return false;
                }
                if (item.UserListId != precedingItem.UserListId)
                {
                    _logger.LogInformation($"The list {precedingItem.UserListId} of the preceding item {precedingItem.UserItemId} is different that than list {item.UserListId} of the item to move {item.UserItemId}");
                    return false;
                }
            }

            _logger.LogInformation($"Updating order of item {item.UserItemId} [{item.Description}] to come after item {precedingItem?.UserItemId} [{precedingItem?.Description}]");
            await _userItemRepository.UpdateOrderAsync(item, precedingItem);
            return true;
        }
    }
}