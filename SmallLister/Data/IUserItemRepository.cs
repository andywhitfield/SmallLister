using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Actions;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserItemRepository
    {
        Task<UserItem> GetItemAsync(UserAccount user, int userItemId);
        Task<(List<UserItem> UserItems, int PageNumber, int PageCount)> GetItemsAsync(UserAccount user, UserList list, UserItemFilter filter = null, int? pageNumber = null, int? pageSize = null);
        Task<List<UserItem>> GetItemsOnNoListAsync(UserAccount user);
        Task AddItemAsync(UserAccount user, UserList list, string description, string notes, DateTime? dueDate, ItemRepeat? repeat, IUserActionsService userActions);
        Task UpdateOrderAsync(UserItem item, UserItem precedingItem);
        Task UpdateOrderAsync(UserAccount user, UserList list, ItemSortOrder? sortOrder);
        Task SaveAsync(UserItem item, UserList newList, IUserActionsService userActions);
        Task<List<UserItem>> FindItemsByQueryAsync(UserAccount user, string searchQuery);
    }
}