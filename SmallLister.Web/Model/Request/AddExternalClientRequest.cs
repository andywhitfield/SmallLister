using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request;

public class AddExternalClientRequest
{
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Uri { get; set; }
}