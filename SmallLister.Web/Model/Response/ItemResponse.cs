using System;

namespace SmallLister.Web.Model.Response;

public class ItemResponse(string itemId, string description, DateTime? dueDate, string? notes)
{
    public string ItemId { get; } = itemId;
    public string Description { get; } = description;
    public DateTime? DueDate { get; } = dueDate;
    public string? Notes { get; } = notes;
}