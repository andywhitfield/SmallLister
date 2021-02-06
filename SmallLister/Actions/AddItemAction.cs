using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SmallLister.Actions.Serialization;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class AddItemAction : IUserAction
    {
        public static AddItemAction Create(string data) => new AddItemAction(DataModel.Deserialize(data));

        private readonly DataModel _model;

        public AddItemAction(UserItem userItem, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> savedItemSortOrders) =>
            _model = new DataModel
            {
                UserItemAdded = new UserItemDataModel(userItem),
                SortOrders = SortOrders.From(savedItemSortOrders)
            };
        private AddItemAction(DataModel model) => _model = model;

        public string Description => $"Add '{DescriptionText(_model.UserItemAdded.Description)}'";

        public UserActionType ActionType => UserActionType.AddItem;
        public Task<string> GetDataAsync() => Task.FromResult(DataModel.Serialize(_model));

        public UserItemDataModel GetUserItemAdded() => DataModel.Copy(_model).UserItemAdded;
        public IEnumerable<SortOrders> GetSortOrders() => (IEnumerable<SortOrders>)DataModel.Copy(_model).SortOrders ?? new SortOrders[0];

        private static string DescriptionText(string value) =>
            (string.IsNullOrEmpty(value) || value.Length < 16) ? value : $"{value.Substring(0, 12)}...";

        private class DataModel
        {
            public static DataModel Copy(DataModel model) => Deserialize(Serialize(model));
            public static string Serialize(DataModel model) => JsonSerializer.Serialize(model);
            public static DataModel Deserialize(string serializedModel) => JsonSerializer.Deserialize<DataModel>(serializedModel);

            public UserItemDataModel UserItemAdded { get; set; }
            public List<SortOrders> SortOrders { get; set; }
        }
    }
}