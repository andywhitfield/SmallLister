using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model
{
    public class UserAccount
    {
        public int UserAccountId { get; set; }
        [Required]
        public string AuthenticationUri { get; set; }
        public int? LastSelectedUserListId { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}