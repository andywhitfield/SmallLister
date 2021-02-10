using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model
{
    public abstract class BaseViewModel
    {
        protected BaseViewModel(HttpContext context, string pageTitle = null)
        {
            IsLoggedIn = context.User?.Identity?.IsAuthenticated ?? false;
            PageTitle = pageTitle ?? "";
        }

        public bool IsLoggedIn { get; }
        public string PageTitle { get; }
    }
}