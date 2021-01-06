using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class UserItemRepository : IUserItemRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<UserItemRepository> _logger;

        public UserItemRepository(SqliteDataContext context, ILogger<UserItemRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<UserItem> GetItemAsync(UserAccount user, int userItemId) =>
            _context.UserItems.SingleOrDefaultAsync(i => i.UserItemId == userItemId && i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null);

        public Task<List<UserItem>> GetItemsAsync(UserAccount user, UserList list, UserItemFilter filter = null)
        {
            var query = _context.UserItems.Where(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null);
            Func<IQueryable<UserItem>, IQueryable<UserItem>> orderByClause;
            if (list != null)
            {
                query = query.Where(i => i.UserListId == list.UserListId);
                orderByClause = q => q.OrderBy(i => i.SortOrder);
            }
            else
            {
                orderByClause = q => q.OrderBy(i => i.UserList.SortOrder).ThenBy(i => i.SortOrder);
            }

            if (filter != null)
            {
                var today = DateTime.Today;
                Func<IQueryable<UserItem>, IQueryable<UserItem>> dueListOrdering = q => q.OrderBy(i => i.NextDueDate).ThenBy(i => i.UserList.SortOrder).ThenBy(i => i.SortOrder);
                if (filter.Overdue && filter.DueToday)
                {
                    query = query.Where(i => i.NextDueDate <= today);
                    orderByClause = dueListOrdering;
                }
                else if (filter.Overdue)
                {
                    query = query.Where(i => i.NextDueDate < today);
                    orderByClause = dueListOrdering;
                }
                else if (filter.DueToday)
                {
                    query = query.Where(i => i.NextDueDate == today);
                    orderByClause = dueListOrdering;
                }
            }

            return orderByClause(query).ToListAsync();
        }

        public async Task AddItemAsync(UserAccount user, UserList list, string description, string notes, DateTime? dueDate, ItemRepeat? repeat)
        {
            var maxSortOrder = await GetMaxSortOrderAsync(user, list?.UserListId);
            _context.UserItems.Add(new UserItem
            {
                UserAccount = user,
                UserList = list,
                Description = description,
                Notes = notes,
                NextDueDate = dueDate,
                Repeat = repeat,
                SortOrder = maxSortOrder + 1
            });
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(UserItem item, UserItem precedingItem)
        {
            var items = _context.UserItems
                .Where(i => i.UserItemId != item.UserItemId && i.UserAccountId == item.UserAccountId && i.UserListId == item.UserListId && i.CompletedDateTime == null && i.DeletedDateTime == null)
                .OrderBy(i => i.SortOrder);
            var sortOrder = 0;
            var now = DateTime.UtcNow;

            if (precedingItem == null)
                UpdateItemSortOrder(item, sortOrder++, now);

            await foreach (var i in items.AsAsyncEnumerable())
            {
                UpdateItemSortOrder(i, sortOrder++, now);
                if (precedingItem?.UserItemId == i.UserItemId)
                    UpdateItemSortOrder(item, sortOrder++, now);
            }
            await _context.SaveChangesAsync();

            void UpdateItemSortOrder(UserItem itemToUpdate, int newSortOrder, DateTime lastUpdateDateTime)
            {
                if (itemToUpdate.SortOrder == newSortOrder)
                    return;
                itemToUpdate.SortOrder = newSortOrder;
                itemToUpdate.LastUpdateDateTime = lastUpdateDateTime;
            }
        }

        public async Task SaveAsync(UserItem item, UserList newList)
        {
            if (item.DeletedDateTime == null && item.UserListId != newList?.UserListId)
            {
                item.UserListId = newList?.UserListId;
                item.SortOrder = (await GetMaxSortOrderAsync(item.UserAccount, item.UserListId)) + 1;
            }
            
            item.LastUpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private async Task<int> GetMaxSortOrderAsync(UserAccount user, int? listId) =>
            (await _context.UserItems
                .Where(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.UserListId == listId)
                .MaxAsync(i => (int?)i.SortOrder)) ?? -1;
    }
}