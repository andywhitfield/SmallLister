using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserListRepository
    {
        Task<UserList> GetListAsync(UserAccount user, int userListId);
        Task<List<UserList>> GetListsAsync(UserAccount user);
        Task AddListAsync(UserAccount user, string name);
        Task SaveAsync(UserList list);
        Task UpdateOrderAsync(UserList list, UserList precedingList);
    }
}