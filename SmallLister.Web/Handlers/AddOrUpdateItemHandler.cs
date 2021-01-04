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
    public class AddOrUpdateItemHandler : IRequestHandler<AddOrUpdateUserItemRequest, bool>
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

        public async Task<bool> Handle(AddOrUpdateUserItemRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            UserList list = null;
            if (request.Request.List.HasValue)
            {
                list = await _userListRepository.GetListAsync(user, request.Request.List.Value);
                if (list == null)
                    return false;
            }

            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(request.Request.Due))
            {
                if (!DateTime.TryParseExact(request.Request.Due, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var due))
                    return false;
                dueDate = due.Date;
            }

            _logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {request.Request.Description}; due={dueDate}; repeat={request.Request.Repeat}; notes={request.Request.Notes}");
            await _userItemRepository.AddItemAsync(user, list, request.Request.Description?.Trim(), request.Request.Notes?.Trim(), dueDate, request.Request.Repeat);

            return true;
        }
    }
}