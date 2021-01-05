using System.Collections.Generic;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetItemForEditResponse
    {
        public static readonly GetItemForEditResponse InvalidResponse = new GetItemForEditResponse();

        private GetItemForEditResponse() => IsValid = false;
        public GetItemForEditResponse(UserItemModel userItem, IEnumerable<UserListModel> lists, UserListModel selectedList)
        {
            IsValid = userItem != null && lists != null && selectedList != null;
            UserItem = userItem;
            Lists = lists;
            SelectedList = selectedList;
        }

        public bool IsValid { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public UserListModel SelectedList { get; }
        public UserItemModel UserItem { get; }
    }
}