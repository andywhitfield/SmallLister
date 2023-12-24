using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class EditItemRequestHandler(ILogger<EditItemRequestHandler> logger, IUserAccountRepository userAccountRepository,
    IUserItemRepository userItemRepository, IUserListRepository userListRepository, IUserActionsService userActionsService,
    IWebhookQueueRepository webhookQueueRepository)
    : IRequestHandler<EditItemRequest, bool>
{
    public async Task<bool> Handle(EditItemRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        var item = await userItemRepository.GetItemAsync(user, request.UserItemId);
        if (item == null)
        {
            logger.LogInformation($"Could not find item {request.UserItemId}");
            return false;
        }

        logger.LogInformation($"Updating item {item.UserItemId}:" +
            $" Description[{item.Description}=>{request.Model.Description}]" +
            $" List[{item.UserListId}=>{request.Model.List}]" +
            $" Repeat[{item.Repeat}=>{request.Model.Repeat}]" +
            $" Notes[{item.Notes}=>{request.Model.Notes}]" +
            $" NextDueDate[{item.NextDueDate}=>{request.Model.Due}]");

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
                    logger.LogInformation($"Could not find list {request.Model.List}");
                    return false;
                }
            }

            if (!AddItemRequestHandler.TryGetDueDate(request.Model.Due, out var dueDate))
            {
                logger.LogInformation($"Could not parse due date {request.Model.Due}");
                return false;
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
        return true;
    }
}