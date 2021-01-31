using System.Collections.Generic;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class AddItemAction : IUserAction
    {
        private readonly UserItem _userItem;
        private readonly IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> _savedItemSortOrders;
        public AddItemAction(UserItem userItem, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> savedItemSortOrders)
        {
            _userItem = userItem;
            _savedItemSortOrders = savedItemSortOrders;
        }
    }
}