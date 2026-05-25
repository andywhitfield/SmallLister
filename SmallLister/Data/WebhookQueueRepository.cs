using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;
using SmallLister.Webhook;

namespace SmallLister.Data;

public class WebhookQueueRepository(ILogger<WebhookQueueRepository> logger, SqliteDataContext context,
    IWebhookNotification webhookNotification)
    : IWebhookQueueRepository
{
    public IAsyncEnumerable<UserListWebhookQueue> GetUnsentUserListWebhookQueuesAsync() =>
        context.UserListWebhookQueue
            .Include(x => x.UserList)
            .ThenInclude(x => x.UserAccount)
            .Where(x => x.DeletedDateTime == null && x.SentDateTime == null)
            .OrderBy(x => x.CreatedDateTime)
            .AsAsyncEnumerable();

    public IAsyncEnumerable<UserItemWebhookQueue> GetUnsentUserItemWebhookQueuesAsync() =>
        context.UserItemWebhookQueue
            .Include(x => x.UserItem)
            .ThenInclude(x => x.UserAccount)
            .Where(x => x.DeletedDateTime == null && x.SentDateTime == null)
            .OrderBy(x => x.CreatedDateTime)
            .AsAsyncEnumerable();

    public async Task OnListChangeAsync(UserAccount user, UserList list, WebhookEventType eventType)
    {
        if (await context.UserWebhooks.AnyAsync(wh =>
            wh.UserAccountId == user.UserAccountId &&
            wh.WebhookType == WebhookType.ListChange &&
            wh.DeletedDateTime == null))
        {
            logger.LogInformation("ListChange webhook exists for this user {UserAccountId}, adding list change {ListUserListId} to queue", user.UserAccountId, list.UserListId);
            context.UserListWebhookQueue.Add(new()
            {
                UserListId = list.UserListId,
                UserList = list,
                EventType = eventType
            });
            await context.SaveChangesAsync();

            webhookNotification.Notify();
        }
    }

    public async Task OnListItemChangeAsync(UserAccount user, UserItem userItem, WebhookEventType eventType)
    {
        if (await context.UserWebhooks.AnyAsync(wh =>
            wh.UserAccountId == user.UserAccountId &&
            wh.WebhookType == WebhookType.ListItemChange &&
            wh.DeletedDateTime == null))
        {
            logger.LogInformation("ListItemChange webhook exists for this user {UserAccountId}, adding user item change {UserItemId} to queue", user.UserAccountId, userItem.UserItemId);
            context.UserItemWebhookQueue.Add(new()
            {
                UserItemId = userItem.UserItemId,
                UserItem = userItem,
                EventType = eventType
            });
            await context.SaveChangesAsync();

            webhookNotification.Notify();
        }
    }

    public Task SentAsync(UserListWebhookQueue userListWebhookQueue, string payload, DateTime? sentTime = null)
    {
        userListWebhookQueue.LastUpdateDateTime = DateTime.UtcNow;
        userListWebhookQueue.SentPayload = payload;
        userListWebhookQueue.SentDateTime = sentTime ?? DateTime.UtcNow;
        return context.SaveChangesAsync();
    }

    public Task SentAsync(UserItemWebhookQueue userItemWebhookQueue, string payload, DateTime? sentTime = null)
    {
        userItemWebhookQueue.LastUpdateDateTime = DateTime.UtcNow;
        userItemWebhookQueue.SentPayload = payload;
        userItemWebhookQueue.SentDateTime = sentTime ?? DateTime.UtcNow;
        return context.SaveChangesAsync();
    }

    public Task<UserItemWebhookQueue?> GetLastSentUserItemWebHookAsync(int userItemId)
        => context.UserItemWebhookQueue
            .Where(x =>
                x.SentDateTime != null &&
                x.DeletedDateTime == null &&
                x.UserItemId == userItemId)
            .OrderByDescending(x => x.UserItemWebhookQueueId)
            .FirstOrDefaultAsync();
}
