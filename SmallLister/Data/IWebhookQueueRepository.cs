using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data;

public interface IWebhookQueueRepository
{
    IAsyncEnumerable<UserListWebhookQueue> GetUnsentUserListWebhookQueuesAsync();
    IAsyncEnumerable<UserItemWebhookQueue> GetUnsentUserItemWebhookQueuesAsync();
    Task OnListChangeAsync(UserAccount user, UserList list, WebhookEventType eventType);
    Task OnListItemChangeAsync(UserAccount user, UserItem userItem, WebhookEventType eventType);
    Task SentAsync(UserListWebhookQueue userListWebhookQueue, string payload, DateTime? sentTime = null);
    Task SentAsync(UserItemWebhookQueue userItemWebhookQueue, string payload, DateTime? sentTime = null);
}
