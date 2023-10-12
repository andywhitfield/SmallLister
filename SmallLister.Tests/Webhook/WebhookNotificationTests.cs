using System;
using System.Threading;
using System.Threading.Tasks;
using SmallLister.Webhook;
using Xunit;

namespace SmallLister.Tests.Webhook;

public class WebhookNotificationTests
{
    [Fact]
    public void Notify_should_cancel_token()
    {
        WebhookNotification webhookNotification = new();
        var token = webhookNotification.GetNewNotificationToken();
        Assert.False(token.IsCancellationRequested);

        webhookNotification.Notify();
        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void Notify_should_cancel_all_outstanding_tokens()
    {
        WebhookNotification webhookNotification = new();
        var token1 = webhookNotification.GetNewNotificationToken();
        var token2 = webhookNotification.GetNewNotificationToken();
        var token3 = webhookNotification.GetNewNotificationToken();
        Assert.False(token1.IsCancellationRequested);
        Assert.False(token2.IsCancellationRequested);
        Assert.False(token3.IsCancellationRequested);

        webhookNotification.Notify();
        Assert.True(token1.IsCancellationRequested);
        Assert.True(token2.IsCancellationRequested);
        Assert.True(token3.IsCancellationRequested);

        var token4 = webhookNotification.GetNewNotificationToken();
        var token5 = webhookNotification.GetNewNotificationToken();
        Assert.True(token1.IsCancellationRequested);
        Assert.True(token2.IsCancellationRequested);
        Assert.True(token3.IsCancellationRequested);
        Assert.False(token4.IsCancellationRequested);
        Assert.False(token5.IsCancellationRequested);

        webhookNotification.Notify();
        Assert.True(token1.IsCancellationRequested);
        Assert.True(token2.IsCancellationRequested);
        Assert.True(token3.IsCancellationRequested);
        Assert.True(token4.IsCancellationRequested);
        Assert.True(token5.IsCancellationRequested);
    }

    [Fact]
    public void Can_wait_on_token()
    {
        WebhookNotification webhookNotification = new();
        var token = webhookNotification.GetNewNotificationToken();
        var failed = false;

        // setup a thread to wait for the token to be signalled (cancelled)
        // the waiting thread waits for up to 2 seconds
        // our main thread then waits for the waiting thread to start,
        // waits another 100ms, then notifies
        var waitTaskStarted = new ManualResetEventSlim (false);
        var waitTask = new Thread(async () =>
        {
            try
            {
                waitTaskStarted.Set();
                await Task.Delay(TimeSpan.FromSeconds(2), token);
                failed = true;
            }
            catch (OperationCanceledException)
            {
            }
        });
        waitTask.Start();
        Assert.True(waitTaskStarted.Wait(TimeSpan.FromSeconds(1)));
        Thread.Sleep(100);

        webhookNotification.Notify();
        waitTask.Join(TimeSpan.FromSeconds(1));

        Assert.False(failed, "Expected Task.Delay to be cancelled");
        Assert.True(token.IsCancellationRequested);
    }
}
