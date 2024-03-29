using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Home;

public class LoginViewModel(HttpContext context, string? returnUrl) : BaseViewModel(context)
{
    public string? ReturnUrl { get; } = returnUrl;
}