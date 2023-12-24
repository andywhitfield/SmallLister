using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request;

public class AddOrUpdateListRequest
{
    [Required]
    public required string Name { get; set; }
}