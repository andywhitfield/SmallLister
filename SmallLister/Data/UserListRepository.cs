using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserListRepository : IUserListRepository
{
    private readonly SqliteDataContext _context;

    public UserListRepository(SqliteDataContext context) => _context = context;

    public Task<UserList> GetListAsync(UserAccount user, int userListId) =>
        _context.UserLists.SingleOrDefaultAsync(l => l.UserListId == userListId && l.UserAccount == user && l.DeletedDateTime == null);

    public Task<List<UserList>> GetListsAsync(UserAccount user) =>
        _context.UserLists.Where(l => l.UserAccount == user && l.DeletedDateTime == null).OrderBy(l => l.SortOrder).ToListAsync();

    public async Task<(int OverdueCount, int DueCount, int TotalCount, int TotalWithDueDateCount, IDictionary<int, int> ListCounts)> GetListCountsAsync(UserAccount user)
    {
        var today = DateTime.Today;
        var overdue = await _context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.NextDueDate != null && ((i.PostponedUntilDate == null && i.NextDueDate.Value.Date < today) || (i.PostponedUntilDate != null && i.PostponedUntilDate.Value < today)));
        var due = await _context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.NextDueDate != null && ((i.PostponedUntilDate == null && i.NextDueDate.Value.Date == today) || (i.PostponedUntilDate != null && i.PostponedUntilDate.Value == today)));
        var total = await _context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null);
        var totalWithDueDate = await _context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.NextDueDate != null);
        var countByListId = await _context.UserItems
            .Where(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.UserListId != null)
            .GroupBy(i => i.UserListId)
            .Select(g => new { UserListId = g.Key.Value, Count = g.Count() })
            .ToDictionaryAsync(s => s.UserListId, s => s.Count);
        return (overdue, due, total, totalWithDueDate, countByListId);
    }

    public async Task<UserList> AddListAsync(UserAccount user, string name)
    {
        var maxSortOrder = await GetMaxSortOrderAsync(user);
        var newEntity = _context.UserLists.Add(new()
        {
            UserAccount = user,
            Name = name,
            SortOrder = maxSortOrder + 1
        });
        await _context.SaveChangesAsync();
        return newEntity.Entity;
    }

    public Task SaveAsync(UserList list)
    {
        list.LastUpdateDateTime = DateTime.UtcNow;
        return _context.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(UserList list, UserList precedingList)
    {
        var lists = _context.UserLists
            .Where(l => l.UserListId != list.UserListId && l.UserAccountId == list.UserAccountId && l.DeletedDateTime == null)
            .OrderBy(l => l.SortOrder);
        var sortOrder = 0;
        var now = DateTime.UtcNow;

        if (precedingList == null)
            UpdateListSortOrder(list, sortOrder++, now);

        await foreach (var l in lists.AsAsyncEnumerable())
        {
            UpdateListSortOrder(l, sortOrder++, now);
            if (precedingList?.UserListId == l.UserListId)
                UpdateListSortOrder(list, sortOrder++, now);
        }
        await _context.SaveChangesAsync();

        static void UpdateListSortOrder(UserList listToUpdate, int newSortOrder, DateTime lastUpdateDateTime)
        {
            if (listToUpdate.SortOrder == newSortOrder)
                return;
            listToUpdate.SortOrder = newSortOrder;
            listToUpdate.LastUpdateDateTime = lastUpdateDateTime;
        }
    }

    private async Task<int> GetMaxSortOrderAsync(UserAccount user) =>
        (await _context.UserLists.Where(l => l.UserAccount == user && l.DeletedDateTime == null).MaxAsync(l => (int?)l.SortOrder)) ?? -1;
}