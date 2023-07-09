using System.Collections.Generic;
using System.Threading;

namespace SmallLister.Webhook;

public class WebhookNotification : IWebhookNotification
{
    private readonly List<CancellationTokenSource> _cancellationTokenSources = new();

    public void Notify()
    {
        List<CancellationTokenSource> toRemove = new();
        _cancellationTokenSources.ForEach(cts =>
        {
            if (cts.IsCancellationRequested)
            {
                cts.Dispose();
                toRemove.Add(cts);
            }
            else
            {
                cts.Cancel();
            }
        });

        foreach (var cts in toRemove)
            _cancellationTokenSources.Remove(cts);
    }

    public CancellationToken GetNewNotificationToken()
    {
        CancellationTokenSource cts = new();
        _cancellationTokenSources.Add(cts);
        return cts.Token;
    }
}
