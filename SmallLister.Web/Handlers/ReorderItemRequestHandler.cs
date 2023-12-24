using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class ReorderItemRequestHandler(ILogger<ReorderItemRequestHandler> logger,
    IUserAccountRepository userAccountRepository, IUserItemRepository userItemRepository,
    IUserActionsService userActionsService)
    : IRequestHandler<ReorderItemRequest, bool>
{
    public async Task<bool> Handle(ReorderItemRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        var item = await userItemRepository.GetItemAsync(user, request.UserItemId);
        if (item == null)
        {
            logger.LogInformation($"Could not find item {request.UserItemId}");
            return false;
        }
        if (item.UserListId == null)
        {
            logger.LogInformation($"Item {request.UserItemId} has no list - can only order items on a specific list not the implicit 'all' list");
            return false;
        }

        UserItem? precedingItem = null;
        if (request.Model.SortOrderPreviousListItemId != null)
        {
            precedingItem = await userItemRepository.GetItemAsync(user, request.Model.SortOrderPreviousListItemId.Value);
            if (precedingItem == null)
            {
                logger.LogInformation($"Could not find preceding item {request.Model.SortOrderPreviousListItemId}");
                return false;
            }
            if (item.UserListId != precedingItem.UserListId)
            {
                logger.LogInformation($"The list {precedingItem.UserListId} of the preceding item {precedingItem.UserItemId} is different that than list {item.UserListId} of the item to move {item.UserItemId}");
                return false;
            }
        }

        logger.LogInformation($"Updating order of item {item.UserItemId} [{item.SortOrder}/{item.Description}] to come after item {precedingItem?.UserItemId} [{precedingItem?.SortOrder}/{precedingItem?.Description}]");
        await userItemRepository.UpdateOrderAsync(user, item, precedingItem, userActionsService);
        return true;
    }
}