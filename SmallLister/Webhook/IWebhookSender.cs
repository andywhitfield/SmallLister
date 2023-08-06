using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Webhook;

public interface IWebhookSender
{
    Task<IEnumerable<(T WebhookToSend, string PayloadSent)>> SendAsync<T>(IEnumerable<(UserAccount UserAccount, T WebhookToSend)> webhooksToSend, WebhookType webhookType);
}
