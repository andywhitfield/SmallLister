using System.Threading;

namespace SmallLister.Webhook;

public interface IWebhookNotification
{
    void Notify();
    CancellationToken GetNewNotificationToken();
}
