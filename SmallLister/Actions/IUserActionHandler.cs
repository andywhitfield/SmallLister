using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public interface IUserActionHandler<out T> where T : IUserAction
    {
        bool CanHandle(UserAction userAction, bool forUndo);
        T GetUserAction(UserAction userAction);
        Task<bool> HandleAsync(UserAccount user, UserAction userAction, bool forUndo);
    }
}