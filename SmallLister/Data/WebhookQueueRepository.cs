using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;
using SmallLister.Webhook;

namespace SmallLister.Data;

public class WebhookQueueRepository : IWebhookQueueRepository
{
    private readonly ILogger<WebhookQueueRepository> _logger;
    private readonly SqliteDataContext _context;
    private readonly IWebhookNotification _webhookNotification;

    public WebhookQueueRepository(ILogger<WebhookQueueRepository> logger, SqliteDataContext context,
        IWebhookNotification webhookNotification)
    {
        _logger = logger;
        _context = context;
        _webhookNotification = webhookNotification;
    }

    public async Task OnListChangeAsync(UserAccount user, UserList list, WebhookEventType eventType)
    {
        if (await _context.UserWebhooks.AnyAsync(wh =>
            wh.UserAccountId == user.UserAccountId &&
            wh.WebhookType == WebhookType.ListChange &&
            wh.DeletedDateTime == null))
        {
            _logger.LogInformation($"ListChange webhook exists for this user {user.UserAccountId}, adding list change {list.UserListId} to queue");
            _context.UserListWebhookQueue.Add(new()
            {
                UserListId = list.UserListId,
                EventType = eventType
            });
            await _context.SaveChangesAsync();

            _webhookNotification.Notify();
        }
    }

    public async Task OnListItemChangeAsync(UserAccount user, UserItem userItem, WebhookEventType eventType)
    {
        if (await _context.UserWebhooks.AnyAsync(wh =>
            wh.UserAccountId == user.UserAccountId &&
            wh.WebhookType == WebhookType.ListItemChange &&
            wh.DeletedDateTime == null))
        {
            _logger.LogInformation($"ListItemChange webhook exists for this user {user.UserAccountId}, adding user item change {userItem.UserItemId} to queue");
            _context.UserItemWebhookQueue.Add(new()
            {
                UserItemId = userItem.UserItemId,
                EventType = eventType
            });
            await _context.SaveChangesAsync();

            _webhookNotification.Notify();
        }
    }
}
