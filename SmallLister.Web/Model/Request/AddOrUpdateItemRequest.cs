using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using MediatR;
using SmallLister.Model;

namespace SmallLister.Web.Model.Request
{
    public class AddOrUpdateItemRequest : IRequest<bool>
    {
        public int? List { get; set; }
        [Required]
        public string Description { get; set; }
        public string Due { get; set; }
        public ItemRepeat? Repeat { get; set; }
        public string Notes { get; set; }
        [NotMapped]
        public ClaimsPrincipal User { get; set; }
    }
}