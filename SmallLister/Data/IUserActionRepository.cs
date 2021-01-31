using System.Threading.Tasks;
using SmallLister.Actions;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserActionRepository
    {
        Task CreateAsync(UserAccount user, string description, UserActionType actionType, string data);
    }
}