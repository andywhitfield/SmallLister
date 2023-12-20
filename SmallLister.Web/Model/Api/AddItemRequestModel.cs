using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Api;

public class AddItemRequestModel(string listId, string description, DateTime? dueDate, string? notes)
{
    [Required]
    public string ListId { get; } = listId;
    [Required]
    public string Description { get; } = description;
    public DateTime? DueDate { get; } = dueDate;
    public string? Notes { get; } = notes;
}