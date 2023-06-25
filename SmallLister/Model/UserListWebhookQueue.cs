using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model;

public class UserListWebhookQueue
{
    public int UserListWebhookQueueId { get; set; }
    public int UserListId { get; set; }
    [Required]
    public UserList UserList { get; set; }
    public WebhookEventType EventType { get; set; }
    public string SentPayload { get; set; }
    public DateTime? SentDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}
