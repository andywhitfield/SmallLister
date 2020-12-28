using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request
{
    public class AddOrUpdateListRequest
    {
        [Required]
        public string Name { get; set; }
    }
}