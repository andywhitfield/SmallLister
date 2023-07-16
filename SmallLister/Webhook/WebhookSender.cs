using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmallLister.Webhook;

public class WebhookSender : IWebhookSender
{
    public Task<IEnumerable<(T WebhookToSend, string PayloadSent)>> SendAsync<T>(IEnumerable<T> webhooksToSend) =>
        Task.FromResult(webhooksToSend.Select(x => (x, "TODO")));
}
