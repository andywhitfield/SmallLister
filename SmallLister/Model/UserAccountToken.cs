using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Model;

public class UserAccountToken
{
    public int UserAccountTokenId { get; set; }
    public int UserAccountApiAccessId { get; set; }
    [Required]
    public required UserAccountApiAccess UserAccountApiAccess { get; set; }
    [Required]
    public required string TokenData { get; set; }
    public DateTime ExpiryDateTime { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}