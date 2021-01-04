using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class AddItemHandler : IRequestHandler<AddItemRequest, bool>
    {
        private readonly ILogger<AddItemHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public AddItemHandler(ILogger<AddItemHandler> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
            IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<bool> Handle(AddItemRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            UserList list = null;
            if (request.Model.List.HasValue)
            {
                list = await _userListRepository.GetListAsync(user, request.Model.List.Value);
                if (list == null)
                    return false;
            }

            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(request.Model.Due))
            {
                if (!DateTime.TryParseExact(request.Model.Due, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var due))
                    return false;
                dueDate = due.Date;
            }

            _logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {request.Model.Description}; due={dueDate}; repeat={request.Model.Repeat}; notes={request.Model.Notes}");
            await _userItemRepository.AddItemAsync(user, list, request.Model.Description?.Trim(), request.Model.Notes?.Trim(), dueDate, request.Model.Repeat);

            return true;
        }
    }
}