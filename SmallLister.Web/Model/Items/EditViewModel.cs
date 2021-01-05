using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Items
{
    public class EditViewModel : BaseViewModel
    {
        public EditViewModel(HttpContext context, UserItemModel itemToEdit, IEnumerable<UserListModel> lists, UserListModel selectedList) : base(context)
        {
            ItemToEdit = itemToEdit;
            Lists = lists;
            SelectedList = selectedList;
        }
        public UserItemModel ItemToEdit { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public UserListModel SelectedList { get; }
    }
}