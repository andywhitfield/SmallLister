using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
        }

        public UserListModel SelectedList { get; set; }
        public IEnumerable<UserListModel> Lists { get; set; }
        public IEnumerable<UserItemModel> Items { get; set; }

        public IndexViewModel SetCssClasses()
        {
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

            return this;
        }
    }
}