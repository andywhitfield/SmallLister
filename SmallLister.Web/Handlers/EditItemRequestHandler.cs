using MediatR;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class EditItemRequestHandler(ILogger<EditItemRequestHandler> logger, IUserAccountRepository userAccountRepository,
    IUserItemRepository userItemRepository, IUserListRepository userListRepository, IUserActionsService userActionsService,
    IWebhookQueueRepository webhookQueueRepository)
    : IRequestHandler<EditItemRequest, (bool, string)>
{
    public async Task<(bool, string)> Handle(EditItemRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        var item = await userItemRepository.GetItemAsync(user, request.UserItemId);
        if (item == null)
        {
            logger.LogInformation("Could not find item {UserItemId}", request.UserItemId);
            return (false, "");
        }

        logger.LogInformation("Updating item {UserItemId}:" +
            " Description[{Description}=>{RequestDescription}]" +
            " List[{UserListId}=>{RequestList}]" +
            " Repeat[{Repeat}=>{RequestRepeat}]" +
            " Notes[{Notes}=>{RequestNotes}]" +
            " NextDueDate[{NextDueDate}=>{RequestDue}]",
            item.UserItemId,
            item.Description, request.Model.Description,
            item.UserListId, request.Model.List,
            item.Repeat, request.Model.Repeat,
            item.Notes, request.Model.Notes,
            item.NextDueDate, request.Model.Due);

        if (request.Model.Cancel ?? false)
        {
            logger.LogTrace("Edit cancelled, redirecting to list view");
            return (true, GetRedirectUrl(user, item));
        }

        UserList? list = null;
        if (request.Model.Delete ?? false)
        {
            item.DeletedDateTime = DateTime.UtcNow;
        }
        else
        {
            item.UserAccount = user;
            item.Description = request.Model.Description;
            item.Notes = request.Model.Notes;
            item.Repeat = request.Model.Repeat;

            if (!string.IsNullOrEmpty(request.Model.List) && int.TryParse(request.Model.List, out var listId))
            {
                list = await userListRepository.GetListAsync(user, listId);
                if (list == null)
                {
                    logger.LogInformation("Could not find list {RequestList}", request.Model.List);
                    return (false, "");
                }
            }

            if (!AddItemRequestHandler.TryGetDueDate(request.Model.Due, out var dueDate))
            {
                logger.LogInformation("Could not parse due date {RequestDue}", request.Model.Due);
                return (false, "");
            }

            if ((request.Model.Snooze ?? false) && item.NextDueDate.HasValue)
            {
                item.PostponedUntilDate = (item.PostponedUntilDate ?? item.NextDueDate)?.Date.AddDays(1);
            }
            else
            {
                if (item.NextDueDate != dueDate)
                {
                    item.PostponedUntilDate = null;
                }
                item.NextDueDate = dueDate;
            }
        }

        await userItemRepository.SaveAsync(item, list, userActionsService);
        await webhookQueueRepository.OnListItemChangeAsync(user, item, item.DeletedDateTime.HasValue ? WebhookEventType.Delete : WebhookEventType.Modify);
        return (true, GetRedirectUrl(user, item));
    }

    internal static string GetRedirectUrl(UserAccount user, UserItem item)
    {
        if (user.LastSelectedUserListId >= 0 && item.UserListId != user.LastSelectedUserListId)
            return $"~/?list={item.UserListId}";
        return "~/";
    }
}