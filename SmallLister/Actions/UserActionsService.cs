using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Actions;

public class UserActionsService(
    ILogger<UserActionsService> logger,
    IUserActionRepository userActionRepository,
    IEnumerable<IUserActionHandler<IUserAction>> userActionHandlers)
    : IUserActionsService
{
    public async Task AddAsync(UserAccount user, IUserAction userAction)
    {
        var data = await userAction.GetDataAsync();
        await userActionRepository.CreateAsync(user, userAction.Description, userAction.ActionType, data);
    }

    public async Task<bool> RedoAsync(UserAccount user)
    {
        var (_, userActionToRedo) = await userActionRepository.GetUndoRedoActionAsync(user);
        if (userActionToRedo == null)
        {
            logger.LogInformation("No redo action available, nothing to do");
            return false;
        }

        var redoSuccess = await UndoRedoAsync(user, userActionToRedo, false);
        if (redoSuccess)
            await userActionRepository.SetActionRedoneAsync(userActionToRedo);
        return redoSuccess;
    }

    public async Task<bool> UndoAsync(UserAccount user)
    {
        var (userActionToUndo, _) = await userActionRepository.GetUndoRedoActionAsync(user);
        if (userActionToUndo == null)
        {
            logger.LogInformation("No undo action available, nothing to do");
            return false;
        }

        var undoSuccess = await UndoRedoAsync(user, userActionToUndo, true);
        if (undoSuccess)
            await userActionRepository.SetActionUndoneAsync(userActionToUndo);
        return undoSuccess;
    }

    public IUserAction GetAction(UserAction userAction)
    {
        var handler = userActionHandlers.FirstOrDefault(h => h.CanHandle(userAction, false));
        if (handler == null)
        {
            logger.LogWarning("Could not find action handler for type {ActionType}. Id={UserActionId}", userAction.ActionType, userAction.UserActionId);
            throw new NotSupportedException();
        }
        return handler.GetUserAction(userAction);
    }

    private Task<bool> UndoRedoAsync(UserAccount user, UserAction userAction, bool forUndo)
    {
        var handler = userActionHandlers.FirstOrDefault(h => h.CanHandle(userAction, forUndo));
        if (handler == null)
        {
            logger.LogWarning("Could not find action handler for type {ActionType}, can't undo/redo. Id={UserActionId}", userAction.ActionType, userAction.UserActionId);
            return Task.FromResult(false);
        }

        return handler.HandleAsync(user, userAction, forUndo);
    }
}
