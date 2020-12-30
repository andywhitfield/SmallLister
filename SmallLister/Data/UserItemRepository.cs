using System;
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