using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Handlers
{
    public class GetListItemsRequestHandler : IRequestHandler<GetListItemsRequest, GetListItemsResponse>
    {
        private readonly ILogger<GetListItemsRequestHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public GetListItemsRequestHandler(ILogger<GetListItemsRequestHandler> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
            IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        public async Task<GetListItemsResponse> Handle(GetListItemsRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var userLists = await _userListRepository.GetListsAsync(user);
            var (overdueCount, dueCount, totalCount, userListCounts) = await _userListRepository.GetListCountsAsync(user);
            var lists = userLists
                .Select(l => new UserListModel { UserListId = l.UserListId.ToString(), Name = l.Name, CanAddItems = true, ItemCount = userListCounts.TryGetValue(l.UserListId, out var listCount) ? listCount : 0, ItemSortOrder = l.ItemSortOrder })
                .Prepend(new UserListModel { Name = "All", UserListId = IndexViewModel.AllList, CanAddItems = true, ItemCount = totalCount });
            var hasDueItems = overdueCount > 0 || dueCount > 0;
            if (hasDueItems)
            {
                var name = overdueCount > 0 && dueCount > 0
                    ? $"{overdueCount} overdue and {dueCount} due today"
                    : overdueCount > 0 ? $"{overdueCount} overdue" : $"{dueCount} due today";
                lists = lists.Prepend(new UserListModel { Name = name, UserListId = IndexViewModel.DueList, CanAddItems = false, ItemCount = overdueCount + dueCount });
            }
            lists = lists.ToList();

            IEnumerable<UserItem> items;
            UserListModel selectedList;
            int pageNumber, pageCount;
            if (request.List != null)
            {
                if (request.List == IndexViewModel.AllList || (request.List == IndexViewModel.DueList && !hasDueItems))
                {
                    (selectedList, items, pageNumber, pageCount) = await GetAllItemsAsync(user, lists, request.PageNumber);
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, null);
                }
                else if (request.List == IndexViewModel.DueList && hasDueItems)
                {
                    (selectedList, items, pageNumber, pageCount) = await GetDueItemsAsync(user, lists, request.PageNumber);
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, -1);
                }
                else if (!int.TryParse(request.List, out var listId))
                {
                    return GetListItemsResponse.BadRequest;
                }
                else
                {
                    if (!userLists.Any(l => l.UserListId == listId))
                        return GetListItemsResponse.BadRequest;

                    (selectedList, items, pageNumber, pageCount) = await GetListItemsAsync(user, userLists, lists, listId, request.Sort, request.PageNumber);
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, listId);
                }
            }
            else
            {
                selectedList = lists.FirstOrDefault(l => l.UserListId == user.LastSelectedUserListId?.ToString());
                if (selectedList == null)
                    if (user.LastSelectedUserListId == -1 && hasDueItems)
                        (selectedList, items, pageNumber, pageCount) = await GetDueItemsAsync(user, lists, request.PageNumber);
                    else
                        (selectedList, items, pageNumber, pageCount) = await GetAllItemsAsync(user, lists, request.PageNumber);
                else
                    (_, items, pageNumber, pageCount) = await GetListItemsAsync(user, userLists, lists, user.LastSelectedUserListId.Value, request.Sort, request.PageNumber);
            }

            return new GetListItemsResponse(lists, selectedList, items.Select(i => new UserItemModel(i)), new Pagination(pageNumber, pageCount));
        }

        private async Task<(UserListModel ListModel, IEnumerable<UserItem> Items, int PageNumber, int PageCount)> GetAllItemsAsync(UserAccount user, IEnumerable<UserListModel> lists, int? pageNumber)
        {
            var (userItems, resultPageNumber, pageCount) = await _userItemRepository.GetItemsAsync(user, null, pageNumber: pageNumber);
            return (lists.Single(l => l.UserListId == IndexViewModel.AllList), userItems, resultPageNumber, pageCount);
        }

        private async Task<(UserListModel, IEnumerable<UserItem>, int PageNumber, int PageCount)> GetDueItemsAsync(UserAccount user, IEnumerable<UserListModel> lists, int? pageNumber)
        {
            var (userItems, resultPageNumber, pageCount) = await _userItemRepository.GetItemsAsync(user, null, new UserItemFilter { Overdue = true, DueToday = true }, pageNumber);
            return (lists.Single(l => l.UserListId == IndexViewModel.DueList), userItems, resultPageNumber, pageCount);
        }

        private async Task<(UserListModel, IEnumerable<UserItem>, int PageNumber, int PageCount)> GetListItemsAsync(UserAccount user, IEnumerable<UserList> userLists, IEnumerable<UserListModel> lists, int listId, ItemSortOrder? sortOrder, int? pageNumber)
        {
            var selectedList = lists.Single(l => l.UserListId == listId.ToString());
            var list = userLists.FirstOrDefault(l => l.UserListId == listId);

            if (list != null && sortOrder != null)
            {
                await _userItemRepository.UpdateOrderAsync(user, list, sortOrder);

                list.ItemSortOrder = sortOrder;
                await _userListRepository.SaveAsync(list);
                selectedList.ItemSortOrder = sortOrder;
            }

            var (userItems, resultPageNumber, pageCount) = await _userItemRepository.GetItemsAsync(user, list, pageNumber: pageNumber);
            return (selectedList, userItems, resultPageNumber, pageCount);
        }
    }
}