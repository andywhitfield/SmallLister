using SmallLister.Model;

namespace SmallLister.Webhook;

public interface IWebhookSender
{
    Task<IEnumerable<(T WebhookToSend, string PayloadSent)>> SendAsync<T>(IEnumerable<(UserAccount UserAccount, T WebhookToSend)> webhooksToSend, WebhookType webhookType, Func<T, Task>? beforeSendActionAsync = null);
}
