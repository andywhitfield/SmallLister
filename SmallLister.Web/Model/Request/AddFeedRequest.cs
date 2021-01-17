using System.ComponentModel.DataAnnotations;
using SmallLister.Model;

namespace SmallLister.Web.Model.Request
{
    public class AddFeedRequest
    {
        [Required]
        public UserFeedType? Type { get; set; }
        [Required]
        public UserFeedItemDisplay? Display { get; set; }
    }
}