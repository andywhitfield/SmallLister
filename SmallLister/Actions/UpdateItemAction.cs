using System.Collections.Generic;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UpdateItemAction : IUserAction
    {
        private UserItem _originalItem;
        private UserItem _updatedItem;
        private IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> _savedItemSortOrders;
        public UpdateItemAction(UserItem originalItem, UserItem updatedItem, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> savedItemSortOrders)
        {
            _originalItem = originalItem;
            _updatedItem = updatedItem;
            _savedItemSortOrders = savedItemSortOrders;
        }
    }
}