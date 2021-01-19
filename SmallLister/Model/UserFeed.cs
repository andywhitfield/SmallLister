using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model
{
    public class UserFeed
    {
        public int UserFeedId { get; set; }
        [Required]
        public string UserFeedIdentifier { get; set; }
        public int UserAccountId { get; set; }
        [Required]
        public UserAccount UserAccount { get; set; }
        public UserFeedType FeedType { get; set; }
        public UserFeedItemDisplay ItemDisplay { get; set; }
        public int ItemHash { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}