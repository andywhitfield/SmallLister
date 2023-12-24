using MediatR;
using SmallLister.Model;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.RequestResponse.Api;

public class GetListRequest(UserAccount user, string listId) : IRequest<GetListResponse?>
{
    public UserAccount User { get; } = user;
    public string ListId { get; } = listId;
}