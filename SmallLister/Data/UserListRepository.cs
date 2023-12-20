using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserListRepository(SqliteDataContext context) : IUserListRepository
{
    public Task<UserList?> GetListAsync(UserAccount user, int userListId) =>
        context.UserLists.SingleOrDefaultAsync(l => l.UserListId == userListId && l.UserAccount == user && l.DeletedDateTime == null);

    public Task<List<UserList>> GetListsAsync(UserAccount user) =>
        context.UserLists.Where(l => l.UserAccount == user && l.DeletedDateTime == null).OrderBy(l => l.SortOrder).ToListAsync();

    public async Task<(int OverdueCount, int DueCount, int TotalCount, int TotalWithDueDateCount, IDictionary<int, int> ListCounts)> GetListCountsAsync(UserAccount user)
    {
        var today = DateTime.Today;
        var overdue = await context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.NextDueDate != null && ((i.PostponedUntilDate == null && i.NextDueDate.Value.Date < today) || (i.PostponedUntilDate != null && i.PostponedUntilDate.Value < today)));
        var due = await context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.NextDueDate != null && ((i.PostponedUntilDate == null && i.NextDueDate.Value.Date == today) || (i.PostponedUntilDate != null && i.PostponedUntilDate.Value == today)));
        var total = await context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null);
        var totalWithDueDate = await context.UserItems.CountAsync(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.NextDueDate != null);
        var countByListId = await context.UserItems
            .Where(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.UserListId != null)
            .GroupBy(i => i.UserListId)
            .Select(g => new { UserListId = g.Key ?? 0, Count = g.Count() })
            .ToDictionaryAsync(s => s.UserListId, s => s.Count);
        return (overdue, due, total, totalWithDueDate, countByListId);
    }

    public async Task<UserList> AddListAsync(UserAccount user, string name)
    {
        var maxSortOrder = await GetMaxSortOrderAsync(user);
        var newEntity = context.UserLists.Add(new()
        {
            UserAccount = user,
            Name = name,
            SortOrder = maxSortOrder + 1
        });
        await context.SaveChangesAsync();
        return newEntity.Entity;
    }

    public Task SaveAsync(UserList list)
    {
        list.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }

    public async Task UpdateOrderAsync(UserList list, UserList precedingList)
    {
        var lists = context.UserLists
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
        await context.SaveChangesAsync();

        static void UpdateListSortOrder(UserList listToUpdate, int newSortOrder, DateTime lastUpdateDateTime)
        {
            if (listToUpdate.SortOrder == newSortOrder)
                return;
            listToUpdate.SortOrder = newSortOrder;
            listToUpdate.LastUpdateDateTime = lastUpdateDateTime;
        }
    }

    private async Task<int> GetMaxSortOrderAsync(UserAccount user) =>
        (await context.UserLists.Where(l => l.UserAccount == user && l.DeletedDateTime == null).MaxAsync(l => (int?)l.SortOrder)) ?? -1;
}