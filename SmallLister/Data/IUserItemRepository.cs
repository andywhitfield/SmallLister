using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Actions;
using SmallLister.Model;

namespace SmallLister.Data;

public interface IUserItemRepository
{
    Task<UserItem?> GetItemAsync(UserAccount user, int userItemId, bool getCompletedOrDeletedItem = false);
    Task<(List<UserItem> UserItems, int PageNumber, int PageCount)> GetItemsAsync(UserAccount user, UserList? list, UserItemFilter? filter = null, int? pageNumber = null, int? pageSize = null);
    Task<List<UserItem>> GetItemsOnNoListAsync(UserAccount user);
    Task<UserItem> AddItemAsync(UserAccount user, UserList list, string description, string notes, DateTime? dueDate, ItemRepeat? repeat, IUserActionsService userActions);
    Task UpdateOrderAsync(UserAccount user, UserItem item, UserItem precedingItem, IUserActionsService userActions);
    Task UpdateOrderAsync(UserAccount user, UserList list, ItemSortOrder? sortOrder, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> collectItemChanges);
    Task SaveAsync(UserItem item, UserList newList, IUserActionsService userActions);
    Task<List<UserItem>> FindItemsByQueryAsync(UserAccount user, string searchQuery);
}