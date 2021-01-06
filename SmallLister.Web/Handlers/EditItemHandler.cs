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
    public class EditItemHandler : IRequestHandler<EditItemRequest, bool>
    {
        private readonly ILogger<EditItemHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserItemRepository _userItemRepository;
        private readonly IUserListRepository _userListRepository;
        public EditItemHandler(ILogger<EditItemHandler> logger, IUserAccountRepository userAccountRepository,
            IUserItemRepository userItemRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userItemRepository = userItemRepository;
            _userListRepository = userListRepository;
        }

        public async Task<bool> Handle(EditItemRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var item = await _userItemRepository.GetItemAsync(user, request.UserItemId);
            if (item == null)
            {
                _logger.LogInformation($"Could not find item {request.UserItemId}");
                return false;
            }

            _logger.LogInformation($"Updating item {item.UserItemId}:" +
                $" Description[{item.Description}=>{request.Model.Description}]" +
                $" List[{item.UserListId}=>{request.Model.List}]" +
                $" Repeat[{item.Repeat}=>{request.Model.Repeat}]" +
                $" Notes[{item.Notes}=>{request.Model.Notes}]" +
                $" NextDueDate[{item.NextDueDate}=>{request.Model.Due}]");

            UserList list = null;
            if (request.Model.Delete ?? false)
            {
                item.DeletedDateTime = DateTime.UtcNow;
            }
            else
            {
                item.UserAccount = user;
                item.Description = request.Model.Description;
                item.Notes = request.Model.Notes;
                item.Repeat = request.Model.Repeat;

                if (request.Model.List != null)
                {
                    list = await _userListRepository.GetListAsync(user, request.Model.List.Value);
                    if (list == null)
                    {
                        _logger.LogInformation($"Could not find list {request.Model.List}");
                        return false;
                    }
                }

                if (!AddItemHandler.TryGetDueDate(request.Model.Due, out var dueDate))
                {
                    _logger.LogInformation($"Could not parse due date {request.Model.Due}");
                    return false;
                }
                item.NextDueDate = dueDate;
            }

            await _userItemRepository.SaveAsync(item, list);
            return true;
        }
    }
}