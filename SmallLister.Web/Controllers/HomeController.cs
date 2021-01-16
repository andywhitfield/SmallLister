using System.Threading.Tasks;
using System.Web;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator) => _mediator = mediator;

        [Authorize]
        [HttpGet("~/")]
        public async Task<IActionResult> Index([FromQuery] string list, [FromQuery] ItemSortOrder? sort, [FromQuery] int? pageNumber)
        {
            var response = await _mediator.Send(new GetListItemsRequest(User, list, sort, pageNumber));
            if (!response.IsValid)
                return BadRequest();

            return View(new IndexViewModel(HttpContext, response.Lists, response.SelectedList, response.Items, response.Pagination));
        }

        public IActionResult Error() => View(new ErrorViewModel(HttpContext));

        [HttpGet("~/signin")]
        public IActionResult Signin([FromQuery] string returnUrl) => View("Login", new LoginViewModel(HttpContext, returnUrl));

        [HttpPost("~/signin")]
        [ValidateAntiForgeryToken]
        public IActionResult SigninChallenge([FromForm] string returnUrl) => Challenge(new AuthenticationProperties { RedirectUri = $"/signedin?returnUrl={HttpUtility.UrlEncode(returnUrl)}" }, OpenIdConnectDefaults.AuthenticationScheme);

        [Authorize]
        [HttpGet("~/signedin")]
        public async Task<IActionResult> SignedIn([FromQuery] string returnUrl) => Redirect(await _mediator.Send(new SignedInRequest(User, returnUrl)));

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult Signout()
        {
            HttpContext.Session.Clear();
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}