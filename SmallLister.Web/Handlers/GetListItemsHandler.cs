using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers
{
    public class GetListItemsHandler : IRequestHandler<GetListItemsRequest, GetListItemsResponse>
    {
        private readonly ILogger<GetListItemsHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public GetListItemsHandler(ILogger<GetListItemsHandler> logger,
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
                .Select(l => new UserListModel { UserListId = l.UserListId.ToString(), Name = l.Name, CanAddItems = true, ItemCount = userListCounts.TryGetValue(l.UserListId, out var listCount) ? listCount : 0 })
                .Prepend(new UserListModel { Name = "All", UserListId = "all", CanAddItems = true, ItemCount = totalCount });
            if (overdueCount > 0 || dueCount > 0)
            {
                var name = overdueCount > 0 && dueCount > 0
                    ? $"{overdueCount} overdue and {dueCount} due today"
                    : overdueCount > 0 ? $"{overdueCount} overdue" : $"{dueCount} due today";
                lists = lists.Prepend(new UserListModel { Name = name, UserListId = "due", CanAddItems = false, ItemCount = overdueCount + dueCount });
            }
            lists = lists.ToList();
            IEnumerable<UserItemModel> items;
            UserListModel selectedList;
            if (request.List != null)
            {
                if (request.List == "all")
                {
                    selectedList = lists.Single(l => l.UserListId == "all");
                    items = (await _userItemRepository.GetItemsAsync(user, null))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, null);
                }
                else if (request.List == "due")
                {
                    selectedList = lists.Single(l => l.UserListId == "due");
                    items = (await _userItemRepository.GetItemsAsync(user, null, new UserItemFilter { Overdue = true, DueToday = true }))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, -1);
                }
                else if (!int.TryParse(request.List, out var listId))
                    return GetListItemsResponse.BadRequest;
                else
                {
                    if (!userLists.Any(l => l.UserListId == listId))
                        return GetListItemsResponse.BadRequest;

                    selectedList = lists.Single(l => l.UserListId == request.List);
                    items = (await _userItemRepository.GetItemsAsync(user, userLists.FirstOrDefault(l => l.UserListId == listId)))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, listId);
                }
            }
            else
            {
                selectedList = lists.FirstOrDefault(l => l.UserListId == user.LastSelectedUserListId?.ToString());
                if (selectedList == null)
                {
                    if (user.LastSelectedUserListId == -1)
                    {
                        selectedList = lists.Single(l => l.UserListId == "due");
                        items = (await _userItemRepository.GetItemsAsync(user, null, new UserItemFilter { Overdue = true, DueToday = true }))
                            .Select(i => new UserItemModel
                            {
                                UserItemId = i.UserItemId,
                                Description = i.Description,
                                Notes = i.Notes
                            }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    }
                    else
                    {
                        selectedList = lists.Single(l => l.UserListId == "all");
                        items = (await _userItemRepository.GetItemsAsync(user, null))
                            .Select(i => new UserItemModel
                            {
                                UserItemId = i.UserItemId,
                                Description = i.Description,
                                Notes = i.Notes
                            }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    }
                }
                else
                {
                    items = (await _userItemRepository.GetItemsAsync(user, userLists.Single(l => l.UserListId.ToString() == selectedList.UserListId)))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                }
            }

            return new GetListItemsResponse(lists, selectedList, items);
        }
    }
}