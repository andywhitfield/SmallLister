using SmallLister.Model;

namespace SmallLister.Web.Model
{
    public class UserListModel
    {
        public string UserListId { get; set; }
        public string Name { get; set; }
        public string CssClass { get; set; }
        public bool CanAddItems { get; set; }
        public int ItemCount { get; set; }
        public ItemSortOrder? ItemSortOrder { get; set; }
    }
}