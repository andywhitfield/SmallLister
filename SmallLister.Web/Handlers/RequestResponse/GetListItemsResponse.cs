using System.Collections.Generic;
using System.Linq;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse;

public class GetListItemsResponse
{
    public static readonly GetListItemsResponse BadRequest = new();

    private GetListItemsResponse() => IsValid = false;
    public GetListItemsResponse(int dueAndOverdueCount, IEnumerable<UserListModel> lists, UserListModel selectedList, IEnumerable<UserItemModel> items,
        Pagination pagination, string undoAction, string redoAction)
    {
        IsValid = lists != null && items != null;
        DueAndOverdueCount = dueAndOverdueCount;
        Lists = lists ?? Enumerable.Empty<UserListModel>();
        SelectedList = selectedList;
        Items = items ?? Enumerable.Empty<UserItemModel>();
        Pagination = pagination;
        UndoAction = undoAction;
        RedoAction = redoAction;
    }

    public bool IsValid { get; }
    public int DueAndOverdueCount { get; }
    public IEnumerable<UserListModel> Lists { get; } = Enumerable.Empty<UserListModel>();
    public UserListModel? SelectedList { get; }
    public IEnumerable<UserItemModel> Items { get; } = Enumerable.Empty<UserItemModel>();
    public Pagination Pagination { get; } = new(1, 1);
    public string UndoAction { get; } = "";
    public string RedoAction { get; } = "";
}