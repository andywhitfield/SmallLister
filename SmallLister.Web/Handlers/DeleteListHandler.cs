using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class DeleteListHandler : IRequestHandler<DeleteListRequest, bool>
    {
        private readonly ILogger<DeleteListHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        public DeleteListHandler(ILogger<DeleteListHandler> logger, IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }
        public async Task<bool> Handle(DeleteListRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var list = await _userListRepository.GetListAsync(user, request.UserListId);
            if (list == null)
                return false;

            _logger.LogInformation($"Deleting list {list.UserListId} [{list.Name}]");
            list.DeletedDateTime = DateTime.UtcNow;
            await _userListRepository.SaveAsync(list);
            return true;
        }
    }
}