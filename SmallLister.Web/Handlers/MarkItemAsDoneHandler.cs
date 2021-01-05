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
    public class MarkItemAsDoneHandler : IRequestHandler<MarkItemAsDoneRequest, bool>
    {
        private readonly ILogger<MarkItemAsDoneHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserItemRepository _userItemRepository;
        private readonly IUserListRepository _userListRepository;
        public MarkItemAsDoneHandler(ILogger<MarkItemAsDoneHandler> logger, IUserAccountRepository userAccountRepository,
            IUserItemRepository userItemRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userItemRepository = userItemRepository;
            _userListRepository = userListRepository;
        }

        public async Task<bool> Handle(MarkItemAsDoneRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var item = await _userItemRepository.GetItemAsync(user, request.UserItemId);
            if (item == null)
            {
                _logger.LogInformation($"Could not find item {request.UserItemId}");
                return false;
            }

            var list = item.UserListId != null ? await _userListRepository.GetListAsync(user, item.UserListId.Value) : null;
            if (item.NextDueDate == null || item.Repeat == null)
                item.CompletedDateTime = DateTime.UtcNow;
            else
                item.NextDueDate = CalculateNextDueDate(item.NextDueDate.Value, item.Repeat.Value);

            await _userItemRepository.SaveAsync(item, list);
            return true;
        }

        private DateTime CalculateNextDueDate(DateTime dueDate, ItemRepeat repeat) =>
            repeat switch
            {
                ItemRepeat.Daily => dueDate.AddDays(1),
                ItemRepeat.Weekly => dueDate.AddDays(7),
                ItemRepeat.Monthly => dueDate.AddMonths(1),
                ItemRepeat.Yearly => dueDate.AddYears(1),
                _ => throw new ArgumentException($"Unsupported repeat option: {repeat}")
            };
    }
}