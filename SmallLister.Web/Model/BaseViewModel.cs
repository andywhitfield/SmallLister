using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model;

public abstract class BaseViewModel(HttpContext context, string? pageTitle = null)
{
    public bool IsLoggedIn { get; } = context.User?.Identity?.IsAuthenticated ?? false;
    public string PageTitle { get; } = pageTitle ?? "";
}