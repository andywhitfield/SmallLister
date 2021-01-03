using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context, IEnumerable<UserListModel> lists, UserListModel selectedList, IEnumerable<UserItemModel> items)
            : base(context)
        {
            Lists = lists;
            SelectedList = selectedList;
            Items = items;

            foreach (var list in Lists)
            {
                if (list.UserListId == "all")
                    list.CssClass = "sml-list-all ";
                else if (list.UserListId == "due")
                    list.CssClass = "sml-list-due ";
                else
                    list.CssClass = "";

                list.CssClass += list.UserListId == SelectedList?.UserListId ? "sml-selected" : "";
            }
        }

        public UserListModel SelectedList { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public IEnumerable<UserItemModel> Items { get; }
    }
}