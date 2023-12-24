using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse.Api;

namespace SmallLister.Web.Handlers.Api;

public class AddItemApiRequestHandler(ILogger<AddItemApiRequestHandler> logger, IUserListRepository userListRepository,
    IUserItemRepository userItemRepository, IUserActionsService userActionsService)
    : IRequestHandler<AddItemApiRequest, bool>
{
    public async Task<bool> Handle(AddItemApiRequest request, CancellationToken cancellationToken)
    {
        UserList? list = null;
        if (request.Model.ListId != "none")
        {
            if (!int.TryParse(request.Model.ListId, out var listId))
            {
                logger.LogInformation($"Invalid list id {request.Model.ListId}");
                return false;
            }

            list = await userListRepository.GetListAsync(request.User, listId);
            if (list == null)
            {
                logger.LogInformation($"Could not find list {request.Model.ListId}");
                return false;
            }
        }

        logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {request.Model.Description}; due={request.Model.DueDate}; notes={request.Model.Notes}");
        await userItemRepository.AddItemAsync(request.User, list, request.Model.Description?.Trim() ?? "", request.Model.Notes?.Trim(), request.Model.DueDate?.Date, null, userActionsService);

        return true;
    }
}