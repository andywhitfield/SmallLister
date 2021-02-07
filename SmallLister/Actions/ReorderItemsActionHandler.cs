using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class ReorderItemsActionHandler : IUserActionHandler<ReorderItemsAction>
    {
        private readonly ILogger<ReorderItemsActionHandler> _logger;
        private readonly IUserItemRepository _userItemRepository;
        private readonly IUserListRepository _userListRepository;

        public ReorderItemsActionHandler(ILogger<ReorderItemsActionHandler> logger, IUserItemRepository userItemRepository,
            IUserListRepository userListRepository)
        {
            _logger = logger;
            _userItemRepository = userItemRepository;
            _userListRepository = userListRepository;
        }

        public bool CanHandle(UserAction userAction, bool forUndo) => userAction.ActionType == UserActionType.ReorderItems;

        public ReorderItemsAction GetUserAction(UserAction userAction) => ReorderItemsAction.Create(userAction.UserActionData);

        public async Task<bool> HandleAsync(UserAccount user, UserAction userAction, bool forUndo)
        {
            var reorderItemsAction = GetUserAction(userAction);

            foreach (var sortOrder in reorderItemsAction.GetSortOrders())
            {
                var item = await _userItemRepository.GetItemAsync(user, sortOrder.UserItemId);
                if (item == null)
                {
                    _logger.LogWarning($"Cannot find item that was moved [{sortOrder.UserItemId}] as part of undo/redo action: {userAction.UserActionId}");
                    return false;
                }

                var newSortOrder = forUndo ? sortOrder.OriginalSortOrder : sortOrder.UpdatedSortOrder;
                if (item.SortOrder != newSortOrder)
                {
                    item.LastUpdateDateTime = DateTime.UtcNow;
                    item.SortOrder = newSortOrder;
                }

                _logger.LogInformation($"Undo/Redo move - re-applied sort order of item {item.UserItemId}");
            }

            var listSortOrder = reorderItemsAction.GetListSortOrder();
            if (listSortOrder.UserListId != null)
            {
                var list = await _userListRepository.GetListAsync(user, listSortOrder.UserListId.Value);
                if (list == null)
                {
                    _logger.LogWarning($"Cannot find list that was updated [{listSortOrder.UserListId}] as part of item move undo/redo action: {userAction.UserActionId}");
                    return false;
                }

                var newItemSortOrder = forUndo ? listSortOrder.OriginalSortOrder : listSortOrder.UpdatedSortOrder;
                if (list.ItemSortOrder != newItemSortOrder)
                {
                    list.LastUpdateDateTime = DateTime.UtcNow;
                    list.ItemSortOrder = newItemSortOrder;
                }
            }

            return true;
        }
    }
}