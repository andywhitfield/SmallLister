using System.Collections.Generic;
using System.Linq;

namespace SmallLister.Actions.Serialization
{
    public class SortOrders
    {
        public int UserItemId { get; set; }
        public int OriginalSortOrder { get; set; }
        public int UpdatedSortOrder { get; set; }

        public static List<SortOrders> From(IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> list) =>
            list.Select(x => new SortOrders
            {
                UserItemId = x.UserItemId,
                OriginalSortOrder = x.OriginalSortOrder,
                UpdatedSortOrder = x.UpdatedSortOrder
            }).ToList();
    }
}