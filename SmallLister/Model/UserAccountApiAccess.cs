using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model;

public class UserAccountApiAccess
{
    public int UserAccountApiAccessId { get; set; }
    public int ApiClientId { get; set; }
    [Required]
    public required ApiClient ApiClient { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public required UserAccount UserAccount { get; set; }
    [Required]
    public required string RefreshToken { get; set; }
    public DateTime? RevokedDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}