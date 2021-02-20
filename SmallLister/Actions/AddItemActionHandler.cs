using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class AddItemActionHandler : IUserActionHandler<AddItemAction>
    {
        private readonly ILogger<AddItemActionHandler> _logger;
        private readonly IUserItemRepository _userItemRepository;
        private readonly IUserActionRepository _userActionRepository;
        private readonly IUserListRepository _userListRepository;

        public AddItemActionHandler(ILogger<AddItemActionHandler> logger, IUserItemRepository userItemRepository,
            IUserActionRepository userActionRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userItemRepository = userItemRepository;
            _userActionRepository = userActionRepository;
            _userListRepository = userListRepository;
        }

        public AddItemAction GetUserAction(UserAction userAction) => AddItemAction.Create(userAction.UserActionData);

        public bool CanHandle(UserAction userAction, bool forUndo) => userAction.ActionType == UserActionType.AddItem;

        public Task<bool> HandleAsync(UserAccount user, UserAction userAction, bool forUndo)
        {
            var addItemAction = GetUserAction(userAction);
            return forUndo
                ? HandleUndoAsync(user, userAction, addItemAction)
                : HandleRedoAsync(user, userAction, addItemAction);
        }

        private async Task<bool> HandleUndoAsync(UserAccount user, UserAction userAction, AddItemAction addItemAction)
        {
            var userItemAdded = addItemAction.GetUserItemAdded();
            var item = await _userItemRepository.GetItemAsync(user, userItemAdded.UserItemId);
            if (item == null)
            {
                _logger.LogWarning($"Cannot find item that was added: {userItemAdded.UserItemId}");
                return false;
            }

            item.DeletedDateTime = item.LastUpdateDateTime = DateTime.UtcNow;
            _logger.LogInformation($"Undo previous add - deleted item {item.UserItemId}");

            return await UpdateSortOrdersAsync(user, userAction, addItemAction, userItemAdded, s => s.OriginalSortOrder);
        }

        private async Task<bool> HandleRedoAsync(UserAccount user, UserAction userAction, AddItemAction addItemAction)
        {
            var userItemAdded = addItemAction.GetUserItemAdded();
            var item = await _userItemRepository.GetItemAsync(user, userItemAdded.UserItemId, true);
            if (item == null)
            {
                _logger.LogWarning($"Cannot find item that was added: {userItemAdded.UserItemId}");
                return false;
            }

            item.LastUpdateDateTime = DateTime.UtcNow;
            item.DeletedDateTime = null;

            _logger.LogInformation($"Redo previous add - undone deleted item {userItemAdded.UserItemId}");

            return await UpdateSortOrdersAsync(user, userAction, addItemAction, userItemAdded, s => s.UpdatedSortOrder);
        }

        private async Task<bool> UpdateSortOrdersAsync(UserAccount user, UserAction userAction, AddItemAction addItemAction, Serialization.UserItemDataModel userItemAdded, Func<Serialization.SortOrders, int> itemSortOrder)
        {
            foreach (var sortOrder in addItemAction.GetSortOrders())
            {
                // ignore the item that was undone/redone
                if (sortOrder.UserItemId == userItemAdded.UserItemId)
                    continue;

                var item = await _userItemRepository.GetItemAsync(user, sortOrder.UserItemId);
                if (item == null)
                {
                    _logger.LogWarning($"Cannot find item that was moved [{sortOrder.UserItemId}] as part of undo/redo action: {userAction.UserActionId}");
                    return false;
                }

                var newSortOrder = itemSortOrder(sortOrder);
                if (item.SortOrder != newSortOrder)
                {
                    item.LastUpdateDateTime = DateTime.UtcNow;
                    item.SortOrder = newSortOrder;
                }

                _logger.LogInformation($"Undo/Redo add - re-applied sort order of item {item.UserItemId}");
            }

            return true;
        }
    }
}