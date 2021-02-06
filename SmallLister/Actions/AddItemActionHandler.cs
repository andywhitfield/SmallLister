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

        public AddItemActionHandler(ILogger<AddItemActionHandler> logger, IUserItemRepository userItemRepository,
            IUserActionRepository userActionRepository)
        {
            _logger = logger;
            _userItemRepository = userItemRepository;
            _userActionRepository = userActionRepository;
        }

        public AddItemAction GetUserAction(UserAction userAction) => AddItemAction.Create(userAction.UserActionData);

        public bool CanHandle(UserAction userAction, bool forUndo) => userAction.ActionType == UserActionType.AddItem;

        public async Task<bool> HandleAsync(UserAccount user, UserAction userAction, bool forUndo)
        {
            var addItemAction = GetUserAction(userAction);
            var userItemAdded = addItemAction.GetUserItemAdded();
            var item = await _userItemRepository.GetItemAsync(user, userItemAdded.UserItemId);
            if (item == null)
            {
                _logger.LogWarning($"Cannot find item that was added: {userItemAdded.UserAccountId}");
                return false;
            }

            if (forUndo)
            {
                await _userActionRepository.DeleteUserItemAsync(item);
                _logger.LogWarning($"Undo previous add - deleted item {item.UserItemId}");

                foreach (var sortOrder in addItemAction.GetSortOrders())
                {
                    if (sortOrder.UserItemId == userItemAdded.UserItemId)
                        continue;

                    item = await _userItemRepository.GetItemAsync(user, sortOrder.UserItemId);
                    if (item == null)
                    {
                        _logger.LogWarning($"Cannot find item that was moved [{sortOrder.UserItemId}] as part of undoing action: {userAction.UserActionId}");
                        return false;
                    }

                    await _userActionRepository.UpdateUserItemAsync(item, sortOrder.OriginalSortOrder);
                    _logger.LogWarning($"Undo previous add - reverted sort order of item {item.UserItemId}");
                }

                return true;
            }

            return false;
        }
    }
}