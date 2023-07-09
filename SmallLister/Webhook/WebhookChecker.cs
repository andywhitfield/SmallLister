using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Webhook.Model;

namespace SmallLister.Webhook;

public class WebhookChecker : IWebhookChecker
{
    private readonly ILogger<WebhookChecker> _logger;
    private readonly IWebhookQueueRepository _webhookQueueRepository;
    private readonly IWebhookSender _webhookSender;

    public WebhookChecker(ILogger<WebhookChecker> logger,
        IWebhookQueueRepository webhookQueueRepository,
        IWebhookSender webhookSender)
    {
        _logger = logger;
        _webhookQueueRepository = webhookQueueRepository;
        _webhookSender = webhookSender;
    }

    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking for any UserList webhooks to send");
        var userListWebhookQueueByUserListId = await GroupBy(_webhookQueueRepository.GetUnsentUserListWebhookQueuesAsync(), x => x.UserListId);

        _logger.LogInformation($"Got {userListWebhookQueueByUserListId.Count} UserLists in the webhook queue, sending...");
        var webhooksToSend = userListWebhookQueueByUserListId.SelectMany(kv =>
            GetWebhookEvents(kv.Key, kv.Value, wh => wh.EventType).Select(e => new ListChange { ListId = e.Key.ToString(), Event = e.EventType.ToString() }));
        var webhooksSent = (await _webhookSender.SendAsync(webhooksToSend)).ToLookup(s => s.WebhookToSend.ListId);

        foreach (var item in userListWebhookQueueByUserListId.SelectMany(x => x.Value))
        {
            var payloadSent = webhooksSent[item.UserListId.ToString()].FirstOrDefault().PayloadSent;
            await _webhookQueueRepository.SentAsync(item, payloadSent);
        }

        // TODO: and then UserItems...
    }

    private static async Task<Dictionary<int, List<T>>> GroupBy<T>(IAsyncEnumerable<T> items, Func<T, int> key)
    {
        Dictionary<int, List<T>> grouped = new();
        await foreach (var item in items)
        {
            var k = key(item);
            if (!grouped.TryGetValue(k, out var userListWebhookQueues))
            {
                userListWebhookQueues = new();
                grouped[k] = userListWebhookQueues;
            }
            userListWebhookQueues.Add(item);
        }
        return grouped;
    }

    // consolidate the items to send
    // as we're sending a thin event, there's no need to send each change, i.e. if there's
    // been a new & then 2 updates, we only need to send a new event
    // the full logic is therefore:
    // - if there are both new & delete, don't send anything
    // - if there are any deletes, send just one delete
    // - if there are any new, send just one new
    // - if there are any updates, send just one update
    private IEnumerable<(int Key, WebhookEventType EventType)> GetWebhookEvents<T>(int key, IEnumerable<T> items, Func<T, WebhookEventType> getEventType)
    {
        var anyNew = items.Any(i => getEventType(i) == WebhookEventType.New);
        var anyDelete = items.Any(i => getEventType(i) == WebhookEventType.Delete);
        if (anyNew && anyDelete)
        {
            _logger.LogInformation($"Item {key} has been created & deleted - no need to send webhook");
            yield break;
        }
        else if (anyDelete)
            yield return (key, WebhookEventType.Delete);
        else if (anyNew)
            yield return (key, WebhookEventType.New);
        else
            yield return (key, WebhookEventType.Modify);
    }
}