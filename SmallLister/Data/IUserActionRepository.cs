using System;
using System.Threading.Tasks;
using SmallLister.Actions;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserActionRepository
    {
        Task CreateAsync(UserAccount user, string description, UserActionType actionType, string data);
        Task<(UserAction UndoAction, UserAction RedoAction)> GetUndoRedoActionAsync(UserAccount user);
        Task SetActionUndoneAsync(UserAction undoAction);
        Task SetActionRedoneAsync(UserAction redoAction);

        Task AddUserItemAsync(UserAccount user, UserList userList, string description, string notes, DateTime? dueDate, ItemRepeat? repeat, int sortOrder, bool saveChanges = false);
    }
}