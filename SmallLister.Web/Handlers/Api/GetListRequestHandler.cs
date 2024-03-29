using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse.Api;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.Api;

public class GetListRequestHandler(IUserListRepository userListRepository, IUserItemRepository userItemRepository)
    : IRequestHandler<GetListRequest, GetListResponse?>
{
    private const int ApiPageSize = 1000;

    public async Task<GetListResponse?> Handle(GetListRequest request, CancellationToken cancellationToken)
    {
        string listName;
        List<UserItem> items;
        
        if (request.ListId == "none")
        {
            listName = "";
            items = await userItemRepository.GetItemsOnNoListAsync(request.User);
        }
        else
        {
            if (!int.TryParse(request.ListId, out var userListId))
                return null;
            
            var list = await userListRepository.GetListAsync(request.User, userListId);
            if (list == null)
                return null;

            listName = list.Name;
            items = (await userItemRepository.GetItemsAsync(request.User, list, pageSize: ApiPageSize)).UserItems;
        }

        return new GetListResponse(listName, items.Select(i => new ItemResponse(i.UserItemId.ToString(), i.Description ?? "", i.NextDueDate, i.Notes)));
    }
}