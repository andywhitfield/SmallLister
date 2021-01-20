using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Lists
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context, IEnumerable<UserListModel> lists) : base(context) => Lists = lists;
        public IEnumerable<UserListModel> Lists { get; }
    }
}