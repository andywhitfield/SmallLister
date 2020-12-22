using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model
{
    public abstract class BaseViewModel
    {
        protected BaseViewModel(HttpContext context)
        {
            IsLoggedIn = context.User?.Identity?.IsAuthenticated ?? false;
        }

        public bool IsLoggedIn { get; }
    }
}