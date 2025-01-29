using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallLister.Webhook;

namespace SmallLister.Tests.Webhook;

[TestClass]
public class WebhookNotificationTests
{
    [TestMethod]
    public void Notify_should_cancel_token()
    {
        WebhookNotification webhookNotification = new();
        var token = webhookNotification.GetNewNotificationToken();
        Assert.IsFalse(token.IsCancellationRequested);

        webhookNotification.Notify();
        Assert.IsTrue(token.IsCancellationRequested);
    }

    [TestMethod]
    public void Notify_should_cancel_all_outstanding_tokens()
    {
        WebhookNotification webhookNotification = new();
        var token1 = webhookNotification.GetNewNotificationToken();
        var token2 = webhookNotification.GetNewNotificationToken();
        var token3 = webhookNotification.GetNewNotificationToken();
        Assert.IsFalse(token1.IsCancellationRequested);
        Assert.IsFalse(token2.IsCancellationRequested);
        Assert.IsFalse(token3.IsCancellationRequested);

        webhookNotification.Notify();
        Assert.IsTrue(token1.IsCancellationRequested);
        Assert.IsTrue(token2.IsCancellationRequested);
        Assert.IsTrue(token3.IsCancellationRequested);

        var token4 = webhookNotification.GetNewNotificationToken();
        var token5 = webhookNotification.GetNewNotificationToken();
        Assert.IsTrue(token1.IsCancellationRequested);
        Assert.IsTrue(token2.IsCancellationRequested);
        Assert.IsTrue(token3.IsCancellationRequested);
        Assert.IsFalse(token4.IsCancellationRequested);
        Assert.IsFalse(token5.IsCancellationRequested);

        webhookNotification.Notify();
        Assert.IsTrue(token1.IsCancellationRequested);
        Assert.IsTrue(token2.IsCancellationRequested);
        Assert.IsTrue(token3.IsCancellationRequested);
        Assert.IsTrue(token4.IsCancellationRequested);
        Assert.IsTrue(token5.IsCancellationRequested);
    }

    [TestMethod]
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
        Assert.IsTrue(waitTaskStarted.Wait(TimeSpan.FromSeconds(1)));
        Thread.Sleep(100);

        webhookNotification.Notify();
        waitTask.Join(TimeSpan.FromSeconds(1));

        Assert.IsFalse(failed, "Expected Task.Delay to be cancelled");
        Assert.IsTrue(token.IsCancellationRequested);
    }
}
