using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse.Api;

namespace SmallLister.Web.Handlers.Api
{
    public class AddItemApiRequestHandler : IRequestHandler<AddItemApiRequest, bool>
    {
        private readonly ILogger<AddItemApiRequestHandler> _logger;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public AddItemApiRequestHandler(ILogger<AddItemApiRequestHandler> logger, IUserListRepository userListRepository, IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<bool> Handle(AddItemApiRequest request, CancellationToken cancellationToken)
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

            _logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {request.Model.Description}; due={request.Model.DueDate}; notes={request.Model.Notes}");
            await _userItemRepository.AddItemAsync(request.User, list, request.Model.Description?.Trim(), request.Model.Notes?.Trim(), request.Model.DueDate?.Date, null);

            return true;
        }
    }
}