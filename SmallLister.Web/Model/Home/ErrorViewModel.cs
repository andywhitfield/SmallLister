using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class ErrorViewModel : BaseViewModel
    {
        public ErrorViewModel(HttpContext context) : base(context) { }
    }
}