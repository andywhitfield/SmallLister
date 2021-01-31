using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UserActionsService : IUserActionsService
    {
        public Task AddAsync(UserAccount user, IUserAction userAction)
        {
            return Task.CompletedTask;
        }
    }
}