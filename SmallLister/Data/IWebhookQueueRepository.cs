using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data;

public interface IWebhookQueueRepository
{
    Task OnListChangeAsync(UserAccount user, UserList list, WebhookEventType eventType);
    Task OnListItemChangeAsync(UserAccount user, UserItem userItem, WebhookEventType eventType);
}
