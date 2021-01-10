using System.ComponentModel.DataAnnotations;

namespace SmallLister.Web.Model.Request
{
    public class CreateAccessTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}