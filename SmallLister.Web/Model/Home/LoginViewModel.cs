using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home
{
    public class LoginViewModel : BaseViewModel
    {
        public string ReturnUrl { get; }
        public LoginViewModel(HttpContext context, string returnUrl) : base(context) => ReturnUrl = returnUrl;
    }
}