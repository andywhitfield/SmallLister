using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class UserListRepository : IUserListRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<UserListRepository> _logger;

        public UserListRepository(SqliteDataContext context, ILogger<UserListRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<UserList> GetListAsync(UserAccount user, int userListId) =>
            _context.UserLists.SingleOrDefaultAsync(l => l.UserListId == userListId && l.UserAccount == user && l.DeletedDateTime == null);

        public Task<List<UserList>> GetListsAsync(UserAccount user) =>
            _context.UserLists
                .Where(l => l.UserAccount == user && l.DeletedDateTime == null)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

        public async Task AddListAsync(UserAccount user, string name)
        {
            var maxSortOrder = await GetMaxSortOrderAsync(user);
            _context.UserLists.Add(new UserList
            {
                UserAccount = user,
                Name = name,
                SortOrder = maxSortOrder + 1
            });
            await _context.SaveChangesAsync();
        }

        public Task SaveAsync(UserList list)
        {
            list.LastUpdateDateTime = DateTime.UtcNow;
            return _context.SaveChangesAsync();
        }

        private async Task<int> GetMaxSortOrderAsync(UserAccount user) =>
            (await _context.UserLists.Where(l => l.UserAccount == user && l.DeletedDateTime == null).MaxAsync(l => (int?)l.SortOrder)) ?? -1;
    }
}