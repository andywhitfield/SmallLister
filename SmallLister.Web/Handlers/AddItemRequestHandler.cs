using System.Globalization;
using MediatR;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class AddItemRequestHandler(
    ILogger<AddItemRequestHandler> logger,
    IUserAccountRepository userAccountRepository,
    IUserListRepository userListRepository,
    IUserItemRepository userItemRepository,
    IUserActionsService userActionsService,
    IWebhookQueueRepository webhookQueueRepository)
    : IRequestHandler<AddItemRequest, bool>
{
    public async Task<bool> Handle(AddItemRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        UserList? list = null;
        if (!string.IsNullOrEmpty(request.Model.List) && request.Model.List == "new")
        {
            // use 'all', rather than reject the add request
            if (string.IsNullOrWhiteSpace(request.Model.Newlist))
                logger.LogWarning("New list was selected, but no name for the list provided. The item will be added to the 'all' list.");
            else
                list = await userListRepository.AddListAsync(user, request.Model.Newlist.Trim());
        }
        else if (!string.IsNullOrEmpty(request.Model.List) && int.TryParse(request.Model.List, out var listId))
        {
            list = await userListRepository.GetListAsync(user, listId);
            if (list == null)
            {
                logger.LogInformation("Could not find list {List}", request.Model.List);
                return false;
            }
        }

        if (!TryGetDueDate(request.Model.Due, out var dueDate))
        {
            logger.LogInformation("Could not parse due date {Due}", request.Model.Due);
            return false;
        }

        logger.LogInformation("Adding item to list {UserListId} [{Name}]: {Description}; due={DueDate}; repeat={Repeat}; notes={Notes}",
            list?.UserListId, list?.Name, request.Model.Description, dueDate, request.Model.Repeat, request.Model.Notes);
        var listItem = await userItemRepository.AddItemAsync(user, list, request.Model.Description?.Trim() ?? "", request.Model.Notes?.Trim(), dueDate, request.Model.Repeat, userActionsService);
        await webhookQueueRepository.OnListItemChangeAsync(user, listItem, WebhookEventType.New);

        return true;
    }

    public static bool TryGetDueDate(string? due, out DateTime? dueDate)
    {
        dueDate = null;
        if (!string.IsNullOrWhiteSpace(due))
        {
            if (!DateTime.TryParseExact(due, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal, out var parsed))
            {
                return false;
            }

            dueDate = parsed.Date;
        }

        return true;
    }
}