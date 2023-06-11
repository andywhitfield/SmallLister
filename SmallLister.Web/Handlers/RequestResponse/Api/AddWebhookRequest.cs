using MediatR;
using SmallLister.Model;
using SmallLister.Web.Model.Api;

namespace SmallLister.Web.Handlers.RequestResponse.Api;

public class AddWebhookRequest : IRequest<bool>
{
    public UserAccount User { get; }
    public AddWebhookRequestModel Model { get; }

    public AddWebhookRequest(UserAccount user, AddWebhookRequestModel model)
    {
        User = user;
        Model = model;
    }
}
