using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request;

public class CreateAccessTokenRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}