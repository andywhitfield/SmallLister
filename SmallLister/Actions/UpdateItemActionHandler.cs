using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UpdateItemActionHandler : IUserActionHandler<UpdateItemAction>
    {
        private readonly ILogger<UpdateItemActionHandler> _logger;
        private readonly IUserItemRepository _userItemRepository;

        public UpdateItemActionHandler(ILogger<UpdateItemActionHandler> logger, IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userItemRepository = userItemRepository;
        }

        public UpdateItemAction GetUserAction(UserAction userAction) => UpdateItemAction.Create(userAction.UserActionData);

        public bool CanHandle(UserAction userAction, bool forUndo) => userAction.ActionType == UserActionType.UpdateItem;

        public Task<bool> HandleAsync(UserAccount user, UserAction userAction, bool forUndo)
        {
            var updateItemAction = GetUserAction(userAction);
            var updatedUserItem = updateItemAction.GetUpdatedUserItem();

            return forUndo
                ? HandleAsync(user, userAction, updateItemAction, updateItemAction.GetOriginalUserItem(), updatedUserItem.CompletedDateTime == null && updatedUserItem.DeletedDateTime == null, s => s.OriginalSortOrder)
                : HandleAsync(user, userAction, updateItemAction, updatedUserItem, true, s => s.UpdatedSortOrder);
        }

        private async Task<bool> HandleAsync(UserAccount user, UserAction userAction, UpdateItemAction updateItemAction, Serialization.UserItemDataModel userItem, bool shouldItemBeActive, Func<Serialization.SortOrders, int> itemSortOrder)
        {
            var item = await _userItemRepository.GetItemAsync(user, userItem.UserItemId, !shouldItemBeActive);
            if (item == null)
            {
                _logger.LogWarning($"Cannot find item that was updated: {userItem.UserItemId}");
                return false;
            }

            item.LastUpdateDateTime = DateTime.UtcNow;
            item.UserListId = userItem.UserListId;
            item.Description = userItem.Description;
            item.Notes = userItem.Notes;
            item.NextDueDate = userItem.NextDueDate;
            item.PostponedUntilDate = userItem.PostponedUntilDate;
            item.Repeat = userItem.Repeat;
            item.SortOrder = userItem.SortOrder;
            item.CompletedDateTime = userItem.CompletedDateTime;
            item.DeletedDateTime = userItem.DeletedDateTime;

            _logger.LogInformation($"Undo previous item update: {item.UserItemId}");

            return await UpdateSortOrdersAsync(user, userAction, updateItemAction, userItem, itemSortOrder);
        }

        private async Task<bool> UpdateSortOrdersAsync(UserAccount user, UserAction userAction, UpdateItemAction updateItemAction, Serialization.UserItemDataModel userItemUpdate, Func<Serialization.SortOrders, int> itemSortOrder)
        {
            foreach (var sortOrder in updateItemAction.GetSortOrders())
            {
                // ignore the item that was undone/redone
                if (sortOrder.UserItemId == userItemUpdate.UserItemId)
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

                _logger.LogInformation($"Undo/Redo update - re-applied sort order of item {item.UserItemId}");
            }

            return true;
        }
    }
}