using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Handlers
{
    public class AddOrUpdateItemHandler : IRequestHandler<AddOrUpdateItemRequest, bool>
    {
        private readonly ILogger<AddOrUpdateItemHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public AddOrUpdateItemHandler(ILogger<AddOrUpdateItemHandler> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
            IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<bool> Handle(AddOrUpdateItemRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            UserList list = null;
            if (request.List.HasValue)
            {
                list = await _userListRepository.GetListAsync(user, request.List.Value);
                if (list == null)
                    return false;
            }

            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(request.Due))
            {
                if (!DateTime.TryParseExact(request.Due, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var due))
                    return false;
                dueDate = due.Date;
            }

            _logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {request.Description}; due={dueDate}; repeat={request.Repeat}; notes={request.Notes}");
            await _userItemRepository.AddItemAsync(user, list, request.Description?.Trim(), request.Notes?.Trim(), dueDate, request.Repeat);

            return true;
        }
    }
}