using MediatR;
using SmallLister.Model;

namespace SmallLister.Web.Handlers.RequestResponse.Api;

public class DeleteWebhookRequest : IRequest
{
    public UserAccount User { get; }
    public WebhookType WebhookType { get; }

    public DeleteWebhookRequest(UserAccount user, WebhookType webhookType)
    {
        User = user;
        WebhookType = webhookType;
    }
}
