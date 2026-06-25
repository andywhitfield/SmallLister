using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse;

public class GetActionLogResponse(IUserListRepository userListRepository, IUserActionsService userActionsService, UserAccount userAccount, UserAction? currentUndoAction, UserAction? currentRedoAction, IAsyncEnumerable<UserAction> allUndoRedoActions)
{
    public UserAction? CurrentUndoAction => currentUndoAction;
    public UserAction? CurrentRedoAction => currentRedoAction;
    public async IAsyncEnumerable<ActionLogResponse> AllUndoRedoActions()
    {
        await foreach (var undoRedoAction in allUndoRedoActions)
            yield return new ActionLogResponse(userListRepository, userActionsService, userAccount, undoRedoAction);
    }
}

public class ActionLogResponse(IUserListRepository userListRepository, IUserActionsService userActionsService, UserAccount userAccount, UserAction userAction)
{
    public string Description => userAction.Description;
    public DateTime CreatedDateTime => userAction.CreatedDateTime;
    public IAsyncEnumerable<(string CssClass, string ChangeDescription)> ChangeDetails
        => userAction.ActionType switch
        {
            UserActionType.AddItem => GetChangeDetailsForAddedItem(),
            UserActionType.UpdateItem => GetChangeDetailsForUpdatedItem(),
            UserActionType.ReorderItems => GetChangeDetailsForReorderedItem(),
            _ => throw new NotSupportedException($"Unknown / unsupported action: {userAction.UserActionId} / {userAction.ActionType}")
        };

    private async IAsyncEnumerable<(string CssClass, string ChangeDescription)> GetChangeDetailsForAddedItem()
    {
        var action = userActionsService.GetAction(userAction) as AddItemAction ?? throw new NotSupportedException($"Unknown / unsupported add action: {userAction.UserActionId}");
        var userItem = action.GetUserItemAdded();
        if (userItem.Description?.Length >= AddItemAction.DescriptionMaxLength)
            yield return ("", userItem.Description);
        yield return ("", $"List: {(await userListRepository.GetListAsync(userAccount, userItem.UserListId ?? 0))?.Name}");
        if (userItem.Notes != null)
            yield return ("sml-list-item-notes", userItem.Notes ?? "");
        if (userItem.Repeat != null)
            yield return ("", $"Repeat: {UserItemModel.GetRepeatSummary(userItem.Repeat)}");
        if (userItem.NextDueDate != null)
            yield return ("", $"Due: {userItem.NextDueDate?.ToString("yyyy-MM-dd")}");
    }

    private async IAsyncEnumerable<(string CssClass, string ChangeDescription)> GetChangeDetailsForUpdatedItem()
    {
        var action = userActionsService.GetAction(userAction) as UpdateItemAction ?? throw new NotSupportedException($"Unknown / unsupported update action: {userAction.UserActionId}");
        var originalItem = action.GetOriginalUserItem();
        var updatedItem = action.GetUpdatedUserItem();
        if (updatedItem.Description?.Length >= UpdateItemAction.DescriptionMaxLength && updatedItem.Description == originalItem.Description)
            yield return ("", updatedItem.Description);
        if (originalItem.UserListId == updatedItem.UserListId)
        {
            yield return ("", $"List: {(await userListRepository.GetListAsync(userAccount, originalItem.UserListId ?? 0))?.Name}");
            yield return ("", "Change from:");
        }
        else
        {
            yield return ("", "Change from:");
            yield return ("", $"List: {(await userListRepository.GetListAsync(userAccount, originalItem.UserListId ?? 0))?.Name}");
        }

        if (originalItem.DeletedDateTime == null)
        {
            if (originalItem.Description != updatedItem.Description)
                yield return ("", originalItem.Description ?? "");
            if (originalItem.Notes != updatedItem.Notes)
                yield return ("sml-list-item-notes", originalItem.Notes ?? "");
            if (originalItem.Repeat != updatedItem.Repeat)
                yield return ("", $"Repeat: {UserItemModel.GetRepeatSummary(originalItem.Repeat)}");
            if (originalItem.NextDueDate != updatedItem.NextDueDate)
                yield return ("", $"Due: {originalItem.NextDueDate?.ToString("yyyy-MM-dd")}");
            if (originalItem.PostponedUntilDate != updatedItem.PostponedUntilDate)
                yield return ("", $"Postponed until: {originalItem.PostponedUntilDate?.ToString("yyyy-MM-dd")}");
            if (originalItem.CompletedDateTime != updatedItem.CompletedDateTime)
                yield return ("", $"Completed: {originalItem.CompletedDateTime?.ToString("yyyy-MM-dd")}");
        }
        else
        {
            yield return ("", $"Deleted: {originalItem.DeletedDateTime?.ToString("yyyy-MM-dd")}");
        }

        yield return ("", "Changed to:");
        if (originalItem.UserListId != updatedItem.UserListId)
            yield return ("", $"List: {(await userListRepository.GetListAsync(userAccount, updatedItem.UserListId ?? 0))?.Name}");
        if (updatedItem.DeletedDateTime == null)
        {
            if (originalItem.Description != updatedItem.Description)
                yield return ("", updatedItem.Description ?? "");
            if (originalItem.Notes != updatedItem.Notes)
                yield return ("sml-list-item-notes", updatedItem.Notes ?? "");
            if (originalItem.Repeat != updatedItem.Repeat)
                yield return ("", $"Repeat: {UserItemModel.GetRepeatSummary(updatedItem.Repeat)}");
            if (originalItem.NextDueDate != updatedItem.NextDueDate)
                yield return ("", $"Due: {updatedItem.NextDueDate?.ToString("yyyy-MM-dd")}");
            if (originalItem.PostponedUntilDate != updatedItem.PostponedUntilDate)
                yield return ("", $"Postponed until: {updatedItem.PostponedUntilDate?.ToString("yyyy-MM-dd")}");
            if (originalItem.CompletedDateTime != updatedItem.CompletedDateTime)
                yield return ("", $"Completed: {updatedItem.CompletedDateTime?.ToString("yyyy-MM-dd")}");
        }
        else
        {
            yield return ("", $"Deleted: {updatedItem.DeletedDateTime?.ToString("yyyy-MM-dd")}");
        }
    }

    private async IAsyncEnumerable<(string CssClass, string ChangeDescription)> GetChangeDetailsForReorderedItem()
    {
        var action = userActionsService.GetAction(userAction) as ReorderItemsAction ?? throw new NotSupportedException($"Unknown / unsupported reorder action: {userAction.UserActionId}");
        var userList = await userListRepository.GetListAsync(userAccount, action.GetListSortOrder().UserListId ?? 0);
        yield return ("", $"Reordered items on: {userList?.Name}");
    }
}