using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UserActionsService : IUserActionsService
    {
        private readonly ILogger<UserActionsService> _logger;
        private readonly IUserActionRepository _userActionRepository;

        public UserActionsService(ILogger<UserActionsService> logger, IUserActionRepository userActionRepository)
        {
            _userActionRepository = userActionRepository;
            _logger = logger;
        }

        public async Task AddAsync(UserAccount user, IUserAction userAction)
        {
            var data = await userAction.GetDataAsync();
            await _userActionRepository.CreateAsync(user, userAction.Description, userAction.ActionType, data);
        }
    }
}