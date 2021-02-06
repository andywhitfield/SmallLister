using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SmallLister.Actions.Serialization;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UpdateItemAction : IUserAction
    {
        public static UpdateItemAction Create(string data) => new UpdateItemAction(JsonSerializer.Deserialize<DataModel>(data));

        private readonly DataModel _model;

        public UpdateItemAction(UserItem originalItem, UserItem updatedItem, IList<(int UserItemId, int OriginalSortOrder, int UpdatedSortOrder)> savedItemSortOrders) =>
            _model = new DataModel
            {
                OriginalUserItem = new UserItemDataModel(originalItem),
                UpdatedUserItem = new UserItemDataModel(updatedItem),
                SortOrders = SortOrders.From(savedItemSortOrders)
            };
        private UpdateItemAction(DataModel model) => _model = model;

        public string Description => $"Update '{DescriptionText(_model.UpdatedUserItem.Description)}'";
        public UserActionType ActionType => UserActionType.UpdateItem;
        public Task<string> GetDataAsync() => Task.FromResult(JsonSerializer.Serialize(_model));

        private static string DescriptionText(string value) =>
            (string.IsNullOrEmpty(value) || value.Length < 16) ? value : $"{value.Substring(0, 12)}...";

        private class DataModel
        {
            public UserItemDataModel OriginalUserItem { get; set; }
            public UserItemDataModel UpdatedUserItem { get; set; }
            public List<SortOrders> SortOrders { get; set; }
        }
    }
}