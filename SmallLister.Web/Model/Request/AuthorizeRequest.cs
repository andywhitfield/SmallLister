using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request
{
    public class AuthorizeRequest
    {
        [Required]
        public string AppKey { get; set; }
        [Required]
        public string RedirectUri { get; set; }
        [Required]
        public bool AllowApiAuth { get; set; }
    }
}