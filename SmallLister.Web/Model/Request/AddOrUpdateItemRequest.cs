using System.ComponentModel.DataAnnotations;
using SmallLister.Model;

namespace SmallLister.Web.Model.Request;

public class AddOrUpdateItemRequest
{
    public string? List { get; set; }
    [Required]
    public string Description { get; set; } = "";
    public string? Due { get; set; }
    public ItemRepeat? Repeat { get; set; }
    public string? Notes { get; set; }
    public bool? Delete { get; set; }
    public bool? Done { get; set; }
    public bool? Snooze { get; set; }
}