using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UserActionsService : IUserActionsService
    {
        private readonly ILogger<UserActionsService> _logger;
        private readonly IUserActionRepository _userActionRepository;
        private readonly IEnumerable<IUserActionHandler<IUserAction>> _userActionHandlers;

        public UserActionsService(ILogger<UserActionsService> logger, IUserActionRepository userActionRepository,
            IEnumerable<IUserActionHandler<IUserAction>> userActionHandlers)
        {
            _logger = logger;
            _userActionRepository = userActionRepository;
            _userActionHandlers = userActionHandlers;
        }

        public async Task AddAsync(UserAccount user, IUserAction userAction)
        {
            var data = await userAction.GetDataAsync();
            await _userActionRepository.CreateAsync(user, userAction.Description, userAction.ActionType, data);
        }
        public async Task<bool> RedoAsync(UserAccount user)
        {
            var (_, userActionToRedo) = await _userActionRepository.GetUndoRedoActionAsync(user);
            if (userActionToRedo == null)
            {
                _logger.LogInformation("No redo action available, nothing to do");
                return false;
            }

            var redoSuccess = await UndoRedoAsync(user, userActionToRedo, false);
            //await _userActionRepository.SetActionRedoneAsync(userActionToRedo);
            return redoSuccess;
        }

        public async Task<bool> UndoAsync(UserAccount user)
        {
            var (userActionToUndo, _) = await _userActionRepository.GetUndoRedoActionAsync(user);
            if (userActionToUndo == null)
            {
                _logger.LogInformation("No undo action available, nothing to do");
                return false;
            }

            var undoSuccess = await UndoRedoAsync(user, userActionToUndo, true);
            await _userActionRepository.SetActionUndoneAsync(userActionToUndo);
            return undoSuccess;
        }

        private Task<bool> UndoRedoAsync(UserAccount user, UserAction userAction, bool forUndo)
        {
            var handler = _userActionHandlers.FirstOrDefault(h => h.CanHandle(userAction, forUndo));
            if (handler == null)
            {
                _logger.LogWarning($"Could not find action handler for type {userAction.ActionType}, can't undo/redo. Id={userAction.UserActionId}");
                return Task.FromResult(false);
            }

            return handler.HandleAsync(user, userAction, forUndo);
        }
    }
}