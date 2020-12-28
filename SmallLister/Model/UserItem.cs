using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model
{
    public class UserItem
    {
        public int UserItemId { get; set; }
        public int UserAccountId { get; set; }
        [Required]
        public UserAccount UserAccount { get; set; }
        public int? UserListId { get; set; }
        public UserList UserList { get; set; }
        [Required]
        public string Description { get; set; }
        public string Notes { get; set; }
        public DateTime? NextDueDate { get; set; }
        public ItemRepeat? Repeat { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}