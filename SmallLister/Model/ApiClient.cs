using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model
{
    public class ApiClient
    {
        public int ApiClientId { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string AppKey { get; set; }
        [Required]
        public string AppSecretSalt { get; set; }
        [Required]
        public string AppSecretHash { get; set; }
        [Required]
        public string RedirectUri { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int CreatedById { get; set; }
        [Required]
        public UserAccount CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdateDateTime { get; set; }
        public DateTime? DeletedDateTime { get; set; }
    }
}