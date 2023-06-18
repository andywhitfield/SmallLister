using System;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data;

public interface IUserWebhookRepository
{
    Task<UserWebhook> GetWebhookAsync(UserAccount user, WebhookType webhookType);
    Task AddWebhookAsync(UserAccount user, WebhookType webhookType, Uri webhookUri);
    Task DeleteWebhookAsync(UserAccount user, WebhookType webhookType);
}
