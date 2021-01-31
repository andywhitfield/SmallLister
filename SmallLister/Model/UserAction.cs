using System;
using System.ComponentModel.DataAnnotations;
using SmallLister.Actions;

namespace SmallLister.Model
{
    public class UserAction
    {
        public int UserActionId { get; set; }
        public int UserAccountId { get; set; }
        [Required]
        public UserAccount UserAccount { get; set; }
        public int ActionNumber { get; set; }
        public bool IsCurrent { get; set; } = true;
        [Required]
        public UserActionType ActionType { get; set; }
        [Required]
        public string UserActionData { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}