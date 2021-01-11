using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model
{
    public class UserList
    {
        public int UserListId { get; set; }
        public int UserAccountId { get; set; }
        [Required]
        public UserAccount UserAccount { get; set; }
        [Required]
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public ItemSortOrder? ItemSortOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}
