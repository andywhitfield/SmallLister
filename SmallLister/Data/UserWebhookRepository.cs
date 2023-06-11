using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserWebhookRepository : IUserWebhookRepository
{
    private readonly SqliteDataContext _context;

    public UserWebhookRepository(SqliteDataContext context) => _context = context;

    public Task<UserWebhook> GetWebhookAsync(UserAccount user, WebhookType webhookType) =>
        _context.UserWebhooks.FirstOrDefaultAsync(wh =>
            wh.UserAccountId == user.UserAccountId &&
            wh.WebhookType == webhookType &&
            wh.DeletedDateTime == null);

    public async Task AddWebhookAsync(UserAccount user, WebhookType webhookType, Uri webhookUri)
    {
        _context.UserWebhooks.Add(new()
        {
            UserAccount = user,
            WebhookType = webhookType,
            Webhook = webhookUri
        });
        await _context.SaveChangesAsync();
    }
}
