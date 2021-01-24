using System.Collections.Generic;
using SmallLister.Web.Model;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class FindItemResponse
    {
        public FindItemResponse(IEnumerable<UserListModel> lists, IEnumerable<UserItemModel> items)
        {
            Lists = lists;
            Items = items;
        }
        public IEnumerable<UserListModel> Lists { get; }
        public IEnumerable<UserItemModel> Items { get; }
    }
}