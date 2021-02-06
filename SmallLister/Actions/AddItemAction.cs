using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SmallLister.Actions.Serialization;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class AddItemAction : IUserAction
    {
        public static AddItemAction Create(string data) => new AddItemAction(JsonSerializer.Deserialize<DataModel>(data));

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
        public Task<string> GetDataAsync() => Task.FromResult(JsonSerializer.Serialize(_model));

        public UserItemDataModel GetUserItemAdded() => Create(GetDataAsync().Result)._model.UserItemAdded;

        private static string DescriptionText(string value) =>
            (string.IsNullOrEmpty(value) || value.Length < 16) ? value : $"{value.Substring(0, 12)}...";

        private class DataModel
        {
            public UserItemDataModel UserItemAdded { get; set; }
            public List<SortOrders> SortOrders { get; set; }
        }
    }
}