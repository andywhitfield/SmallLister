using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers;

public class FindItemRequestHandler : IRequestHandler<FindItemRequest, FindItemResponse>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserListRepository _userListRepository;
    private readonly IUserItemRepository _userItemRepository;

    public FindItemRequestHandler(
        IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
        IUserItemRepository userItemRepository)
    {
        _userAccountRepository = userAccountRepository;
        _userListRepository = userListRepository;
        _userItemRepository = userItemRepository;
    }

    public async Task<FindItemResponse> Handle(FindItemRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAccountRepository.GetUserAccountAsync(request.User);
        var userLists = await _userListRepository.GetListsAsync(user);
        var (overdueCount, dueCount, totalCount, totalWithDueDateCount, userListCounts) = await _userListRepository.GetListCountsAsync(user);
        var (lists, _) = GetListItemsRequestHandler.GetUserListModels(userLists, overdueCount, dueCount, totalCount, totalWithDueDateCount, userListCounts);

        var items = string.IsNullOrWhiteSpace(request.SearchQuery) ? new List<UserItem>() : await _userItemRepository.FindItemsByQueryAsync(user, request.SearchQuery.Trim());
        return new FindItemResponse(lists, items.Select(i => new UserItemModel(i)));
    }
}