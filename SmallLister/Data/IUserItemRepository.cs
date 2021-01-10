using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserItemRepository
    {
        Task<UserItem> GetItemAsync(UserAccount user, int userItemId);
        Task<List<UserItem>> GetItemsAsync(UserAccount user, UserList list, UserItemFilter filter = null);
        Task<List<UserItem>> GetItemsOnNoListAsync(UserAccount user);
        Task AddItemAsync(UserAccount user, UserList list, string description, string notes, DateTime? dueDate, ItemRepeat? repeat);
        Task UpdateOrderAsync(UserItem item, UserItem precedingItem);
        Task SaveAsync(UserItem item, UserList newList);
    }
}