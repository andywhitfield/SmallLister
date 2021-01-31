using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class AddItemRequestHandler : IRequestHandler<AddItemRequest, bool>
    {
        private readonly ILogger<AddItemRequestHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;
        private readonly IUserActionsService _userActionsService;

        public AddItemRequestHandler(ILogger<AddItemRequestHandler> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
            IUserItemRepository userItemRepository, IUserActionsService userActionsService)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
            _userActionsService = userActionsService;
        }

        public async Task<bool> Handle(AddItemRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            UserList list = null;
            if (!string.IsNullOrEmpty(request.Model.List) && int.TryParse(request.Model.List, out var listId))
            {
                list = await _userListRepository.GetListAsync(user, listId);
                if (list == null)
                {
                    _logger.LogInformation($"Could not find list {request.Model.List}");
                    return false;
                }
            }

            if (!TryGetDueDate(request.Model.Due, out var dueDate))
            {
                _logger.LogInformation($"Could not parse due date {request.Model.Due}");
                return false;
            }

            _logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {request.Model.Description}; due={dueDate}; repeat={request.Model.Repeat}; notes={request.Model.Notes}");
            await _userItemRepository.AddItemAsync(user, list, request.Model.Description?.Trim(), request.Model.Notes?.Trim(), dueDate, request.Model.Repeat, _userActionsService);

            return true;
        }

        public static bool TryGetDueDate(string due, out DateTime? dueDate)
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
}