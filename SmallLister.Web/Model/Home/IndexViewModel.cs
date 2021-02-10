using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class IndexViewModel : BaseViewModel
    {
        public const string AllList = "all";
        public const string DueList = "due";

        public IndexViewModel(HttpContext context, IEnumerable<UserListModel> lists, UserListModel selectedList,
            IEnumerable<UserItemModel> items, Pagination pagination, string undoAction, string redoAction)
            : base(context)
        {
            Lists = lists;
            SelectedList = selectedList;
            Items = items;
            Pagination = pagination;
            UndoAction = undoAction;
            RedoAction = redoAction;

            foreach (var list in Lists)
            {
                if (list.UserListId == AllList)
                    list.CssClass = "sml-list-all ";
                else if (list.UserListId == DueList)
                    list.CssClass = "sml-list-due ";
                else
                    list.CssClass = "";

                list.CssClass += list.UserListId == SelectedList.UserListId ? "sml-selected" : "";
            }
        }

        public UserListModel SelectedList { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public IEnumerable<UserItemModel> Items { get; }
        public bool IsDueListSelected => SelectedList.UserListId == DueList;
        public bool IsAllListSelected => SelectedList.UserListId == AllList;
        public Pagination Pagination { get; }
        public string UndoAction { get; }
        public bool HasUndoAction => !string.IsNullOrEmpty(UndoAction);
        public string RedoAction { get; }
        public bool HasRedoAction => !string.IsNullOrEmpty(RedoAction);
    }
}