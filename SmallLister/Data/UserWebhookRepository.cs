using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserWebhookRepository(SqliteDataContext context) : IUserWebhookRepository
{
    public Task<UserWebhook?> GetWebhookAsync(UserAccount user, WebhookType webhookType) =>
        context.UserWebhooks.FirstOrDefaultAsync(wh =>
            wh.UserAccountId == user.UserAccountId &&
            wh.WebhookType == webhookType &&
            wh.DeletedDateTime == null);

    public async Task AddWebhookAsync(UserAccount user, WebhookType webhookType, Uri webhookUri)
    {
        context.UserWebhooks.Add(new()
        {
            UserAccount = user,
            WebhookType = webhookType,
            Webhook = webhookUri
        });
        await context.SaveChangesAsync();
    }

    public async Task DeleteWebhookAsync(UserAccount user, WebhookType webhookType)
    {
        DateTime now = DateTime.UtcNow;
        await foreach (var wh in context.UserWebhooks.Where(x =>
            x.UserAccountId == user.UserAccountId &&
            x.WebhookType == webhookType &&
            x.DeletedDateTime == null).AsAsyncEnumerable())
        {
            wh.DeletedDateTime = now;
        }
        
        await context.SaveChangesAsync();
    }
}
