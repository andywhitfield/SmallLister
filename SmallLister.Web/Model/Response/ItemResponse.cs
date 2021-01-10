using System;

namespace SmallLister.Web.Model.Response
{
    public class ItemResponse
    {
        public ItemResponse(string itemId, string description, DateTime? dueDate, string notes)
        {
            ItemId = itemId;
            Description = description;
            DueDate = dueDate;
            Notes = notes;
        }
        public string ItemId { get; }
        public string Description { get; }
        public DateTime? DueDate { get; }
        public string Notes { get; }
    }
}