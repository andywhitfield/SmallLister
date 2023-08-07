using MediatR;
using SmallLister.Model;
using SmallLister.Web.Model.Api;

namespace SmallLister.Web.Handlers.RequestResponse.Api;

public class DeleteItemApiRequest : IRequest<bool>
{
    public UserAccount User { get; }
    public AddItemRequestModel Model { get; }
    public DeleteItemApiRequest(UserAccount user, AddItemRequestModel model)
    {
        User = user;
        Model = model;
    }
}