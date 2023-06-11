using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model;

public class UserWebhook
{
    public int UserWebhookId { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public UserAccount UserAccount { get; set; }
    public WebhookType WebhookType { get; set; }
    public Uri Webhook { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}
