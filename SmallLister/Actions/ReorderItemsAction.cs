using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SmallLister.Actions.Serialization;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class ReorderItemsAction : IUserAction
    {
        public static ReorderItemsAction Create(string data) => new ReorderItemsAction(DataModel.Deserialize(data));

        private readonly DataModel _model;

        public ReorderItemsAction(IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> itemSortOrders, (int? UserListId, ItemSortOrder? OriginalSortOrder, ItemSortOrder? UpdatedSortOrder) listSortOrder) =>
            _model = new DataModel
            {
                SortOrders = SortOrders.From(itemSortOrders),
                UserListId = listSortOrder.UserListId,
                UserListOriginalSortOrder = listSortOrder.OriginalSortOrder,
                UserListUpdatedSortOrder = listSortOrder.UpdatedSortOrder
            };
        private ReorderItemsAction(DataModel model) => _model = model;

        public string Description => "Reorder";

        public UserActionType ActionType => UserActionType.ReorderItems;
        public Task<string> GetDataAsync() => Task.FromResult(DataModel.Serialize(_model));

        public IEnumerable<SortOrders> GetSortOrders() => (IEnumerable<SortOrders>)DataModel.Copy(_model).SortOrders ?? new SortOrders[0];
        public (int? UserListId, ItemSortOrder? OriginalSortOrder, ItemSortOrder? UpdatedSortOrder) GetListSortOrder()
        {
            var copy = DataModel.Copy(_model);
            return (copy.UserListId, copy.UserListOriginalSortOrder, copy.UserListUpdatedSortOrder);
        }

        private static string DescriptionText(string value) =>
            (string.IsNullOrEmpty(value) || value.Length < 16) ? value : $"{value.Substring(0, 12)}...";

        private class DataModel
        {
            public static DataModel Copy(DataModel model) => Deserialize(Serialize(model));
            public static string Serialize(DataModel model) => JsonSerializer.Serialize(model);
            public static DataModel Deserialize(string serializedModel) => JsonSerializer.Deserialize<DataModel>(serializedModel);

            public List<SortOrders> SortOrders { get; set; }
            public int? UserListId { get; set; }
            public ItemSortOrder? UserListOriginalSortOrder { get; set; }
            public ItemSortOrder? UserListUpdatedSortOrder { get; set; }
        }
    }
}