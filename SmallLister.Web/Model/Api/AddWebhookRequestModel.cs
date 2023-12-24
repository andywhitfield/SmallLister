using System;
using System.ComponentModel.DataAnnotations;
using SmallLister.Model;

namespace SmallLister.Web.Model.Api;

public class AddWebhookRequestModel
{
    [Required]
    public required Uri Webhook { get; set; }
    [Required]
    public WebhookType WebhookType { get; set; }
}
