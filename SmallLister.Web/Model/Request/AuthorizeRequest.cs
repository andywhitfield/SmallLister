using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request;

public class AuthorizeRequest
{
    [Required]
    public required string AppKey { get; set; }
    [Required]
    public required string RedirectUri { get; set; }
    [Required]
    public bool AllowApiAuth { get; set; }
}