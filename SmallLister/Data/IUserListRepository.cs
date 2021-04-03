using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserListRepository
    {
        Task<UserList> GetListAsync(UserAccount user, int userListId);
        Task<List<UserList>> GetListsAsync(UserAccount user);
        Task<(int OverdueCount, int DueCount, int TotalCount, int TotalWithDueDateCount, IDictionary<int, int> ListCounts)> GetListCountsAsync(UserAccount user);
        Task AddListAsync(UserAccount user, string name);
        Task SaveAsync(UserList list);
        Task UpdateOrderAsync(UserList list, UserList precedingList);
    }
}