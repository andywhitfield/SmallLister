using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class ReorderListRequestHandler(ILogger<ReorderListRequestHandler> logger,
    IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
    : IRequestHandler<ReorderListRequest, bool>
{
    public async Task<bool> Handle(ReorderListRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        var list = await userListRepository.GetListAsync(user, request.UserListId);
        if (list == null)
        {
            logger.LogInformation($"Could not find list {request.UserListId}");
            return false;
        }

        UserList? precedingList = null;
        if (request.Model.SortOrderPreviousListItemId != null)
        {
            precedingList = await userListRepository.GetListAsync(user, request.Model.SortOrderPreviousListItemId.Value);
            if (precedingList == null)
            {
                logger.LogInformation($"Could not find preceding list {request.Model.SortOrderPreviousListItemId}");
                return false;
            }
        }

        logger.LogInformation($"Updating order of list {list.UserListId} [{list.Name}] to come after list {precedingList?.UserListId} [{precedingList?.Name}]");
        await userListRepository.UpdateOrderAsync(list, precedingList);
        return true;
    }
}