using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model
{
    public class UserFeedItem
    {
        public int UserFeedItemId { get; set; }
        public int UserFeedId { get; set; }
        [Required]
        public UserFeed UserFeed { get; set; }
        public int UserItemId { get; set; }
        [Required]
        public UserItem UserItem { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}