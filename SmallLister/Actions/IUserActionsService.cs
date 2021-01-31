using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public interface IUserActionsService
    {
        Task AddAsync(UserAccount user, IUserAction userAction);
    }
}