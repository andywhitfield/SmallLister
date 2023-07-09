using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmallLister.Webhook;

public interface IWebhookSender
{
    Task<IEnumerable<(T WebhookToSend, string PayloadSent)>> SendAsync<T>(IEnumerable<T> webhooksToSend);
}
