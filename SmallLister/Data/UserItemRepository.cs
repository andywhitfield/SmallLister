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

        public Task<List<UserItem>> GetItemsAsync(UserAccount user, UserList list, UserItemFilter filter = null)
        {
            var query = _context.UserItems.Where(i => i.UserAccount == user && i.DeletedDateTime == null);
            if (filter != null)
            {
                var today = DateTime.Today;
                if (filter.Overdue && filter.DueToday)
                    query = query.Where(i => i.NextDueDate <= today);
                else if (filter.Overdue)
                    query = query.Where(i => i.NextDueDate < today);
                else if (filter.DueToday)
                    query = query.Where(i => i.NextDueDate == today);
            }

            if (list == null)
                query = query.OrderBy(i => i.UserList.SortOrder).ThenBy(i => i.SortOrder);
            else
                query = query.Where(i => i.UserListId == list.UserListId).OrderBy(i => i.SortOrder);

            return query.ToListAsync();
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

        private async Task<int> GetMaxSortOrderAsync(UserAccount user, int? listId) =>
            (await _context.UserItems
                .Where(i => i.UserAccount == user && i.DeletedDateTime == null && i.UserListId == listId)
                .MaxAsync(i => (int?)i.SortOrder)) ?? -1;
    }
}