using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Model;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public HomeController(ILogger<HomeController> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository,
            IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        [Authorize]
        [HttpGet("~/")]
        public async Task<IActionResult> Index([FromQuery] string list)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var userLists = await _userListRepository.GetListsAsync(user);
            var lists = userLists.Select(l => new UserListModel { UserListId = l.UserListId, Name = l.Name });
            UserListModel selectedList = null;
            if (list == "due")
            {
            }
            else if (list == "all")
            {
                await _userAccountRepository.SetLastSelectedUserListIdAsync(user, null);
            }
            else if (!string.IsNullOrEmpty(list))
            {
                if (!int.TryParse(list, out var listId))
                    return BadRequest();

                if (!userLists.Any(l => l.UserListId == listId))
                    return BadRequest();

                selectedList = lists.Single(l => l.UserListId == listId);
                await _userAccountRepository.SetLastSelectedUserListIdAsync(user, listId);
            }
            else
            {
                selectedList = lists.FirstOrDefault(l => l.UserListId == user.LastSelectedUserListId);
            }
            var items = (await _userItemRepository.GetItemsAsync(user, userLists.FirstOrDefault(l => l.UserListId == selectedList?.UserListId)))
                .Select(i => new UserItemModel
                {
                    UserItemId = i.UserItemId,
                    Description = i.Description,
                    Notes = i.Notes
                }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));

            return View(new IndexViewModel(HttpContext)
            {
                SelectedList = selectedList,
                Items = items,
                Lists = lists
            });
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