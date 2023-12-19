using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model;

public class UserItemWebhookQueue
{
    public int UserItemWebhookQueueId { get; set; }
    public int UserItemId { get; set; }
    [Required]
    public UserItem UserItem { get; set; }
    public WebhookEventType EventType { get; set; }
    public string? SentPayload { get; set; }
    public DateTime? SentDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}
