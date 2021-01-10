using System;
using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Api
{
    public class AddItemRequestModel
    {
        public AddItemRequestModel(string listId, string description, DateTime? dueDate, string notes)
        {
            ListId = listId;
            Description = description;
            DueDate = dueDate;
            Notes = notes;
        }
        [Required]
        public string ListId { get; }
        [Required]
        public string Description { get; }
        public DateTime? DueDate { get; }
        public string Notes { get; }
    }
}