using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel(HttpContext context) : base(context) { }
    }
}