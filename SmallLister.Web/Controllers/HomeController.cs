using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _mediator;

        public HomeController(ILogger<HomeController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("~/")]
        public async Task<IActionResult> Index([FromQuery] string list)
        {
            var response = await _mediator.Send(new GetListItemsRequest(User, list));
            if (!response.IsValid)
                return BadRequest();

            return View(new IndexViewModel(HttpContext, response.Lists, response.SelectedList, response.Items));
        }

        public IActionResult Error() => View(new ErrorViewModel(HttpContext));

        [HttpGet("~/signin")]
        public IActionResult Signin() => View("Login", new LoginViewModel(HttpContext));

        [HttpPost("~/signin")]
        [ValidateAntiForgeryToken]
        public IActionResult SigninChallenge() => Challenge(new AuthenticationProperties { RedirectUri = "/signedin" }, OpenIdConnectDefaults.AuthenticationScheme);

        [Authorize]
        [HttpGet("~/signedin")]
        public async Task<IActionResult> SignedIn()
        {
            await _mediator.Send(new SignedInRequest(User));
            return Redirect("~/");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult Signout()
        {
            HttpContext.Session.Clear();
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}