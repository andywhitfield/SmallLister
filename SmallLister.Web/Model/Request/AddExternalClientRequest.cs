using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request
{
    public class AddExternalClientRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Uri { get; set; }
    }
}