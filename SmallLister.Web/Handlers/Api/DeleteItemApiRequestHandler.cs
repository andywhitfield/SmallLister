using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse.Api;

namespace SmallLister.Web.Handlers.Api;

public class DeleteItemApiRequestHandler : IRequestHandler<DeleteItemApiRequest, bool>
{
    private readonly ILogger<DeleteItemApiRequestHandler> _logger;
    private readonly IUserListRepository _userListRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly IUserActionsService _userActionsService;

    public DeleteItemApiRequestHandler(ILogger<DeleteItemApiRequestHandler> logger, IUserListRepository userListRepository,
        IUserItemRepository userItemRepository, IUserActionsService userActionsService)
    {
        _logger = logger;
        _userListRepository = userListRepository;
        _userItemRepository = userItemRepository;
        _userActionsService = userActionsService;
    }

    public async Task<bool> Handle(DeleteItemApiRequest request, CancellationToken cancellationToken)
    {
        UserList list = null;
        if (request.Model.ListId != "none")
        {
            if (!int.TryParse(request.Model.ListId, out var listId))
            {
                _logger.LogInformation($"Invalid list id {request.Model.ListId}");
                return false;
            }

            list = await _userListRepository.GetListAsync(request.User, listId);
            if (list == null)
            {
                _logger.LogInformation($"Could not find list {request.Model.ListId}");
                return false;
            }
        }

        _logger.LogInformation($"Deleting item from list {list?.UserListId} [{list?.Name}]: {request.Model.Description}; due={request.Model.DueDate}; notes={request.Model.Notes}");
        var matchingItems = await _userItemRepository.FindItemsByQueryAsync(request.User, request.Model.Description?.Trim());
        var now = DateTime.UtcNow;
        foreach (var matchingItem in matchingItems.Where(i => i.UserListId == list?.UserListId && string.Equals(i.Description?.Trim(), request.Model.Description?.Trim(), System.StringComparison.OrdinalIgnoreCase)))
        {
            matchingItem.DeletedDateTime = now;
            await _userItemRepository.SaveAsync(matchingItem, null, _userActionsService);
        }

        return true;
    }
}