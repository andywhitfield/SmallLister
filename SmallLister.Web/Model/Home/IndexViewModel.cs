using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home;

public class IndexViewModel : BaseViewModel
{
    public const string AllList = "all";
    public const string AllWithDueDateList = "alldue";
    public const string DueList = "due";

    public static void SetListCssClass(IEnumerable<UserListModel> lists, UserListModel? selectedList)
    {
        foreach (var list in lists)
        {
            list.CssClass = list.UserListId switch
            {
                AllList => "sml-list-all ",
                AllWithDueDateList => "sml-list-all ",
                DueList => "sml-list-due ",
                _ => ""
            };

            list.CssClass += list.UserListId == selectedList?.UserListId ? "sml-selected" : "";
        }
    }

    public IndexViewModel(HttpContext context, int dueAndOverdueCount, IEnumerable<UserListModel> lists, UserListModel? selectedList,
        IEnumerable<UserItemModel> items, Pagination pagination, string undoAction, string redoAction)
        : base(context, dueAndOverdueCount > 0 ? $" ({dueAndOverdueCount})" : null)
    {
        Lists = lists;
        SelectedList = selectedList;
        Items = items;
        Pagination = pagination;
        UndoAction = undoAction;
        RedoAction = redoAction;
        SetListCssClass(lists, selectedList);
    }

    public UserListModel? SelectedList { get; }
    public IEnumerable<UserListModel> Lists { get; }
    public IEnumerable<UserItemModel> Items { get; }
    public bool IsDueListSelected => SelectedList?.UserListId == DueList;
    public bool IsAllListSelected => SelectedList?.UserListId == AllList;
    public bool IsAllWithDueDateListSelected => SelectedList?.UserListId == AllWithDueDateList;
    public bool IsUserListSelected => !IsAllListSelected && !IsDueListSelected && !IsAllWithDueDateListSelected;
    public Pagination Pagination { get; }
    public string UndoAction { get; }
    public bool HasUndoAction => !string.IsNullOrEmpty(UndoAction);
    public string RedoAction { get; }
    public bool HasRedoAction => !string.IsNullOrEmpty(RedoAction);
}