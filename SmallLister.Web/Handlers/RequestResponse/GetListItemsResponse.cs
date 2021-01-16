using System.Collections.Generic;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetListItemsResponse
    {
        public static readonly GetListItemsResponse BadRequest = new GetListItemsResponse();

        private GetListItemsResponse() => IsValid = false;
        public GetListItemsResponse(IEnumerable<UserListModel> lists, UserListModel selectedList, IEnumerable<UserItemModel> items,
            Pagination pagination)
        {
            IsValid = lists != null && items != null;
            Lists = lists;
            SelectedList = selectedList;
            Items = items;
            Pagination = pagination;
        }

        public bool IsValid { get; }
        public IEnumerable<UserListModel> Lists { get; }
        public UserListModel SelectedList { get; }
        public IEnumerable<UserItemModel> Items { get; }
        public Pagination Pagination { get; }
    }
}