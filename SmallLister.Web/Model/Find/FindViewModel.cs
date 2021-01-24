using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Find
{
    public class FindViewModel : BaseViewModel
    {
        public FindViewModel(HttpContext context, string findText, IEnumerable<UserListModel> lists,
            IEnumerable<UserItemModel> items) : base(context)
        {
            FindText = findText;
            Lists = lists;
            Items = items;
        }
        public string FindText { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public IEnumerable<UserItemModel> Items { get; }
    }
}