using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _mediator;
        private readonly IUserAccountRepository _userAccountRepository;

        public HomeController(ILogger<HomeController> logger, IMediator mediator,
            IUserAccountRepository userAccountRepository)
        {
            _logger = logger;
            _mediator = mediator;
            _userAccountRepository = userAccountRepository;
        }

        [Authorize]
        [HttpGet("~/")]
        public async Task<IActionResult> Index([FromQuery] string list)
        {
            var request = new GetListItemsRequest(User, list);
            var response = await _mediator.Send(request);
            if (!response.IsValid)
                return BadRequest();

            return View(new IndexViewModel(HttpContext)
            {
                SelectedList = response.SelectedList,
                Items = response.Items,
                Lists = response.Lists
            }.SetCssClasses());
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
            if (await _userAccountRepository.GetUserAccountOrNullAsync(User) == null)
                await _userAccountRepository.CreateNewUserAsync(User);
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