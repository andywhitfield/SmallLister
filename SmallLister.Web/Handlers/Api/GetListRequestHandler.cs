using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse.Api;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Handlers.Api
{
    public class GetListRequestHandler : IRequestHandler<GetListRequest, GetListResponse>
    {
        private readonly ILogger<GetListRequestHandler> _logger;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public GetListRequestHandler(ILogger<GetListRequestHandler> logger, IUserListRepository userListRepository, IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<GetListResponse> Handle(GetListRequest request, CancellationToken cancellationToken)
        {
            string listName;
            List<UserItem> items;
            
            if (request.ListId == "none")
            {
                listName = "";
                items = await _userItemRepository.GetItemsOnNoListAsync(request.User);
            }
            else
            {
                if (!int.TryParse(request.ListId, out var userListId))
                    return null;
                
                var list = await _userListRepository.GetListAsync(request.User, userListId);
                if (list == null)
                    return null;

                listName = list.Name;
                items = await _userItemRepository.GetItemsAsync(request.User, list);
            }

            return new GetListResponse(listName, items.Select(i => new ItemResponse(i.UserItemId.ToString(), i.Description, i.NextDueDate, i.Notes)));
        }
    }
}