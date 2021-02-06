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
            if (forUndo)
            {
                var userItemAdded = addItemAction.GetUserItemAdded();
                var item = await _userItemRepository.GetItemAsync(user, userItemAdded.UserItemId);
                if (item == null)
                {
                    _logger.LogWarning($"Cannot find item that was added: {userItemAdded.UserAccountId}");
                    return false;
                }

                await _userActionRepository.DeleteUserItemAsync(item);
                return true;
            }

            return false;
        }
    }
}