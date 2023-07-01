using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserItemRepository : IUserItemRepository
{
    private const int DefaultPageSize = 100;

    private readonly SqliteDataContext _context;
    private readonly ILogger<UserItemRepository> _logger;

    public UserItemRepository(SqliteDataContext context, ILogger<UserItemRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<UserItem> GetItemAsync(UserAccount user, int userItemId, bool getCompletedOrDeletedItem = false) =>
        _context.UserItems.SingleOrDefaultAsync(i => i.UserItemId == userItemId && i.UserAccount == user && (getCompletedOrDeletedItem || (i.CompletedDateTime == null && i.DeletedDateTime == null)));

    public async Task<(List<UserItem> UserItems, int PageNumber, int PageCount)> GetItemsAsync(UserAccount user, UserList list, UserItemFilter filter = null, int? pageNumber = null, int? pageSize = null)
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
            if (filter.WithDueDate)
            {
                query = query.Where(i => i.NextDueDate != null);
                orderByClause = DueListOrdering;
            }
            if (filter.Overdue && filter.DueToday)
            {
                query = query.Where(i => (i.PostponedUntilDate ?? i.NextDueDate) <= today);
                orderByClause = DueListOrdering;
            }
            else if (filter.Overdue)
            {
                query = query.Where(i => (i.PostponedUntilDate ?? i.NextDueDate) < today);
                orderByClause = DueListOrdering;
            }
            else if (filter.DueToday)
            {
                query = query.Where(i => (i.PostponedUntilDate ?? i.NextDueDate) == today);
                orderByClause = DueListOrdering;
            }

            static IQueryable<UserItem> DueListOrdering(IQueryable<UserItem> q) => q.OrderBy(i => i.PostponedUntilDate ?? i.NextDueDate).ThenBy(i => i.UserList.SortOrder).ThenBy(i => i.SortOrder);
        }

        var total = await query.CountAsync();
        var actualPageSize = pageSize ?? DefaultPageSize;
        var (pageIndex, pageCount) = Paging.GetPageInfo(total, actualPageSize, pageNumber ?? 1);
        _logger.LogTrace($"Getting page index {pageIndex} of {pageCount} total pages, total items: {total}, requested page: {pageNumber}");

        return (await orderByClause(query)
            .Skip(pageIndex * actualPageSize)
            .Take(actualPageSize)
            .ToListAsync(), pageIndex + 1, pageCount);
    }

    public Task<List<UserItem>> GetItemsOnNoListAsync(UserAccount user) => _context.UserItems
        .Where(i => i.UserAccount == user && i.UserList == null && i.CompletedDateTime == null && i.DeletedDateTime == null)
        .OrderBy(i => i.SortOrder).ToListAsync();

    public async Task<UserItem> AddItemAsync(UserAccount user, UserList list, string description, string notes, DateTime? dueDate, ItemRepeat? repeat, IUserActionsService userActions)
    {
        var maxSortOrder = await GetMaxSortOrderAsync(user, list?.UserListId);
        var newUserItem = new UserItem
        {
            UserAccount = user,
            UserList = list,
            Description = description,
            Notes = notes,
            NextDueDate = dueDate,
            Repeat = repeat,
            SortOrder = maxSortOrder + 1
        };
        var newEntity = _context.UserItems.Add(newUserItem);
        await _context.SaveChangesAsync();

        var savedItemSortOrders = new List<(int, int, int)>();
        if (list?.ItemSortOrder != null)
            await UpdateOrderAsync(user, list, list.ItemSortOrder, savedItemSortOrders);

        await userActions.AddAsync(user, new AddItemAction(newUserItem, savedItemSortOrders));
        return newEntity.Entity;
    }

    public async Task UpdateOrderAsync(UserAccount user, UserItem item, UserItem precedingItem, IUserActionsService userActions)
    {
        var savedItemSortOrders = new List<(int, int, int)>();
        var items = _context.UserItems
            .Where(i => i.UserItemId != item.UserItemId && i.UserAccountId == item.UserAccountId && i.UserListId == item.UserListId && i.CompletedDateTime == null && i.DeletedDateTime == null)
            .OrderBy(i => i.SortOrder);
        var sortOrder = 0;
        var now = DateTime.UtcNow;

        if (precedingItem == null)
            UpdateItemSortOrder(item, sortOrder++, now, savedItemSortOrders);

        await foreach (var i in items.AsAsyncEnumerable())
        {
            UpdateItemSortOrder(i, sortOrder++, now, savedItemSortOrders);
            if (precedingItem?.UserItemId == i.UserItemId)
                UpdateItemSortOrder(item, sortOrder++, now, savedItemSortOrders);
        }

        if (savedItemSortOrders.Any())
        {
            var savedListSortOrder = ((int?)null, (ItemSortOrder?)null, (ItemSortOrder?)null);
            var list = item.UserListId == null ? null : await _context.UserLists.SingleOrDefaultAsync(l => l.UserListId == item.UserListId && l.DeletedDateTime == null);
            if (list != null)
            {
                savedListSortOrder = (list.UserListId, list.ItemSortOrder, null);
                list.ItemSortOrder = null;
                list.LastUpdateDateTime = now;
            }

            await _context.SaveChangesAsync();

            await userActions.AddAsync(user, new ReorderItemsAction(savedItemSortOrders, savedListSortOrder));
        }
        else
            _logger.LogInformation("Sort order not changed, nothing to do.");

        static void UpdateItemSortOrder(UserItem itemToUpdate, int newSortOrder, DateTime lastUpdateDateTime, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> collectItemChanges)
        {
            if (itemToUpdate.SortOrder == newSortOrder)
                return;
            collectItemChanges.Add((itemToUpdate.UserItemId, itemToUpdate.SortOrder, newSortOrder));
            itemToUpdate.SortOrder = newSortOrder;
            itemToUpdate.LastUpdateDateTime = lastUpdateDateTime;
        }
    }

    public async Task UpdateOrderAsync(UserAccount user, UserList list, ItemSortOrder? sortOrder, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> collectItemChanges)
    {
        var items = _context.UserItems.Where(i => i.UserAccountId == user.UserAccountId && i.UserListId == list.UserListId && i.CompletedDateTime == null && i.DeletedDateTime == null);
        switch (sortOrder)
        {
            case ItemSortOrder.DueDate:
                items = items.OrderBy(i => i.PostponedUntilDate ?? i.NextDueDate ?? DateTime.MaxValue).ThenBy(i => i.SortOrder);
                break;
            case ItemSortOrder.Description:
                items = items.OrderBy(i => i.Description).ThenBy(i => i.SortOrder);
                break;
            default:
                return;
        }
        var order = 0;
        var now = DateTime.UtcNow;
        await foreach (var item in items.AsAsyncEnumerable())
        {
            var newSortOrder = order++;
            if (item.SortOrder == newSortOrder)
                continue;
            collectItemChanges?.Add((item.UserItemId, item.SortOrder, newSortOrder));
            item.LastUpdateDateTime = now;
            item.SortOrder = newSortOrder;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveAsync(UserItem item, UserList newList, IUserActionsService userActions)
    {
        if (item.DeletedDateTime == null && item.UserListId != newList?.UserListId)
        {
            item.UserListId = newList?.UserListId;
            item.SortOrder = (await GetMaxSortOrderAsync(item.UserAccount, item.UserListId)) + 1;
        }

        item.LastUpdateDateTime = DateTime.UtcNow;
        var originalItem = (UserItem)_context.Entry(item).OriginalValues.ToObject();
        await _context.SaveChangesAsync();

        var savedItemSortOrders = new List<(int, int, int)>();
        if (item.DeletedDateTime == null)
        {
            if (newList != null && newList.ItemSortOrder != null)
            {
                await UpdateOrderAsync(item.UserAccount, newList, newList.ItemSortOrder, savedItemSortOrders);
            }
            else if (item.UserListId != null)
            {
                var list = await _context.UserLists.SingleOrDefaultAsync(l => l.UserListId == item.UserListId && l.DeletedDateTime == null);
                if (list != null)
                {
                    await UpdateOrderAsync(item.UserAccount, list, list.ItemSortOrder, savedItemSortOrders);
                }
            }
        }
        await userActions.AddAsync(item.UserAccount, new UpdateItemAction(originalItem, item, savedItemSortOrders));
    }

    public Task<List<UserItem>> FindItemsByQueryAsync(UserAccount user, string searchQuery) => _context.UserItems
            .Where(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && (EF.Functions.Like(i.Description, $"%{searchQuery}%") || EF.Functions.Like(i.Notes, $"%{searchQuery}%")))
            .OrderByDescending(i => i.LastUpdateDateTime ?? i.CreatedDateTime)
            .ToListAsync();

    private async Task<int> GetMaxSortOrderAsync(UserAccount user, int? listId) =>
        (await _context.UserItems
            .Where(i => i.UserAccount == user && i.CompletedDateTime == null && i.DeletedDateTime == null && i.UserListId == listId)
            .MaxAsync(i => (int?)i.SortOrder)) ?? -1;
}