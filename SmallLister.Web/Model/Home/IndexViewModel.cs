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
    }
}