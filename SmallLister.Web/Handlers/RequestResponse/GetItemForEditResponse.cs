using System.Collections.Generic;
using System.Linq;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse;

public class GetItemForEditResponse
{
    public static readonly GetItemForEditResponse InvalidResponse = new();

    private GetItemForEditResponse() => IsValid = false;

    public GetItemForEditResponse(UserItemModel userItem, IEnumerable<UserListModel> lists, UserListModel selectedList)
    {
        IsValid = userItem != null && lists != null && selectedList != null;
        UserItem = userItem;
        Lists = lists ?? Enumerable.Empty<UserListModel>();
        SelectedList = selectedList;
    }

    public bool IsValid { get; }
    public IEnumerable<UserListModel> Lists { get; } = Enumerable.Empty<UserListModel>();
    public UserListModel? SelectedList { get; }
    public UserItemModel? UserItem { get; }
}