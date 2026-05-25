using System.Text.Json;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Webhook.Model;

namespace SmallLister.Webhook;

public class WebhookChecker(ILogger<WebhookChecker> logger,
    IWebhookQueueRepository webhookQueueRepository,
    IWebhookSender webhookSender)
    : IWebhookChecker
{
    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking for any UserList webhooks to send");
        var userListWebhookQueueByUserListId = await GroupBy(webhookQueueRepository.GetUnsentUserListWebhookQueuesAsync(), x => x.UserListId);

        if (userListWebhookQueueByUserListId.Count > 0)
        {
            logger.LogInformation("Got {UserListWebhookQueueByUserListIdCount} UserLists in the webhook queue, sending...", userListWebhookQueueByUserListId.Count);

            var webhooksToSend = userListWebhookQueueByUserListId.SelectMany(kv =>
                GetWebhookEvents(kv.Key, kv.Value, wh => wh.EventType).Select(e => (kv.Value.First().UserList.UserAccount, Event: new ListChange { ListId = e.Key.ToString(), Event = e.EventType.ToString() })));
            var webhooksToIgnore = userListWebhookQueueByUserListId.Keys.Select(userListId => userListId.ToString()).Except(webhooksToSend.Select(wh => wh.Event.ListId)).ToHashSet();
            var webhooksSent = (await webhookSender.SendAsync(webhooksToSend, WebhookType.ListChange)).ToLookup(s => s.WebhookToSend.ListId);

            logger.LogDebug("Sent {WebhooksSentCount} webhooks, updating queue with payload details", webhooksSent.Count);
            foreach (var item in userListWebhookQueueByUserListId.SelectMany(x => x.Value))
            {
                if (!webhooksToIgnore.Contains(item.UserListId.ToString()) && !webhooksSent.Contains(item.UserListId.ToString()))
                {
                    logger.LogWarning("Did not send webhooks for user list id {UserListId}, not setting user list webhook queue entry as sent", item.UserListId);
                    continue;
                }

                var payloadSent = webhooksSent[item.UserListId.ToString()].FirstOrDefault().PayloadSent;
                await webhookQueueRepository.SentAsync(item, payloadSent);
            }

            logger.LogInformation("Sent all webhooks successfully");
        }
        else
        {
            logger.LogInformation("Got no UserLists in the webhook queue, nothing to do");
        }

        logger.LogDebug("Checking for any UserItem webhooks to send");
        var userItemWebhookQueueByUserItemId = await GroupBy(webhookQueueRepository.GetUnsentUserItemWebhookQueuesAsync(), x => x.UserItemId);

        if (userItemWebhookQueueByUserItemId.Count > 0)
        {
            logger.LogInformation("Got {UserItemWebhookQueueByUserItemIdCount} UserItem in the webhook queue, sending...", userItemWebhookQueueByUserItemId.Count);

            var webhooksToSend = userItemWebhookQueueByUserItemId.SelectMany(kv =>
                GetWebhookEvents(kv.Key, kv.Value, wh => wh.EventType).Select(e => (kv.Value.First().UserItem.UserAccount, Event: new ListItemChange { ListItemId = e.Key.ToString(), ListId = kv.Value.First().UserItem.UserListId?.ToString() ?? "", Event = e.EventType.ToString() })));
            var webhooksToIgnore = userItemWebhookQueueByUserItemId.Keys.Select(userItemId => userItemId.ToString()).Except(webhooksToSend.Select(wh => wh.Event.ListItemId)).ToHashSet();
            var webhooksSent = (await webhookSender.SendAsync(webhooksToSend, WebhookType.ListItemChange, LookupPreviousListItemIdAsync)).ToLookup(s => s.WebhookToSend.ListItemId);

            logger.LogDebug("Sent {WebhooksSentCount} webhooks, updating queue with payload details", webhooksSent.Count);
            foreach (var item in userItemWebhookQueueByUserItemId.SelectMany(x => x.Value))
            {
                if (!webhooksToIgnore.Contains(item.UserItemId.ToString()) && !webhooksSent.Contains(item.UserItemId.ToString()))
                {
                    logger.LogWarning("Did not send webhooks for user item id {UserItemId}, not setting user item webhook queue entry as sent", item.UserItemId);
                    continue;
                }

                var payloadSent = webhooksSent[item.UserItemId.ToString()].FirstOrDefault().PayloadSent;
                logger.LogDebug("Item {UserItemId}, sent payload: {PayloadSent}", item.UserItemId, payloadSent);
                await webhookQueueRepository.SentAsync(item, payloadSent);
            }

            logger.LogInformation("Sent all webhooks successfully");
        }
        else
        {
            logger.LogInformation("Got no UserItems in the webhook queue, nothing to do");
        }
    }

    private async Task LookupPreviousListItemIdAsync(ListItemChange listItemChange)
    {
        var lastSentWebhook = await webhookQueueRepository.GetLastSentUserItemWebHookAsync(int.TryParse(listItemChange.ListItemId, out var listItemId) ? listItemId : 0);
        if (lastSentWebhook != null)
        {
            try
            {
                var sentPayload = JsonSerializer.Deserialize<IEnumerable<ListItemChange>>(lastSentWebhook.SentPayload ?? "");
                if (sentPayload != null)
                    listItemChange.PreviousListId = sentPayload.FirstOrDefault(p => !string.IsNullOrEmpty(p.ListId))?.ListId ?? "";
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deserialize last sent payload for user item webhook queue id {LastSentWebhookUserItemWebhookQueueId}", lastSentWebhook.UserItemWebhookQueueId);
            }
        }
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
            logger.LogInformation("Item {Key} has been created & deleted - no need to send webhook", key);
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