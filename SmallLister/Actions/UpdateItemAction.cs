using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using SmallLister.Actions.Serialization;
using SmallLister.Model;

namespace SmallLister.Actions;

public class UpdateItemAction : IUserAction
{
    public static UpdateItemAction Create(string data) => new(DataModel.Deserialize(data));

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
    public Task<string> GetDataAsync() => Task.FromResult(DataModel.Serialize(_model));

    public UserItemDataModel GetOriginalUserItem() => DataModel.Copy(_model).OriginalUserItem;
    public UserItemDataModel GetUpdatedUserItem() => DataModel.Copy(_model).UpdatedUserItem;
    public IEnumerable<SortOrders> GetSortOrders() => (IEnumerable<SortOrders>?)DataModel.Copy(_model).SortOrders ?? Array.Empty<SortOrders>();

    private static string? DescriptionText(string? value) =>
        (string.IsNullOrEmpty(value) || value.Length < 24) ? value : $"{value.Substring(0, 21)}...";

    private class DataModel
    {
        public static DataModel Copy(DataModel model) => Deserialize(Serialize(model));
        public static string Serialize(DataModel model) => JsonSerializer.Serialize(model);
        public static DataModel Deserialize(string serializedModel)
            => JsonSerializer.Deserialize<DataModel>(serializedModel) ?? throw new InvalidOperationException($"Could not deserialize into DataModel: {serializedModel}");

        public required UserItemDataModel OriginalUserItem { get; set; }
        public required UserItemDataModel UpdatedUserItem { get; set; }
        public List<SortOrders>? SortOrders { get; set; }
    }
}