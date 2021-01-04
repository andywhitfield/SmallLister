using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class UpdateListHandler : IRequestHandler<UpdateListRequest, bool>
    {
        private readonly ILogger<UpdateListHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        public UpdateListHandler(ILogger<UpdateListHandler> logger, IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }

        public async Task<bool> Handle(UpdateListRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var list = await _userListRepository.GetListAsync(user, request.UserListId);
            if (list == null)
                return false;

            _logger.LogInformation($"Updating name of list {list.UserListId} [{list.Name}] to [{request.Model.Name}]");
            list.Name = request.Model.Name;
            await _userListRepository.SaveAsync(list);
            return true;
        }
    }
}