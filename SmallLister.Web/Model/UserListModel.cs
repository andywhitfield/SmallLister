using SmallLister.Model;

namespace SmallLister.Web.Model;

public class UserListModel
{
    public UserListModel(string userListId, string name, bool canAddItems, int itemCount = 0, ItemSortOrder? itemSortOrder = null)
    {
        UserListId = userListId;
        Name = name;
        CanAddItems = canAddItems;
        ItemCount = itemCount;
        ItemSortOrder = itemSortOrder;
    }
    public string UserListId { get; }
    public string Name { get; }
    public bool CanAddItems { get; }
    public int ItemCount { get; }
    public string? CssClass { get; set; }
    public ItemSortOrder? ItemSortOrder { get; set; }
}