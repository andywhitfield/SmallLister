using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
        }
    }
}