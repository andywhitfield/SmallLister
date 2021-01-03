using System.Collections.Generic;
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
            var (overdueCount, dueCount, totalCount, userListCounts) = await _userListRepository.GetListCountsAsync(user);
            var lists = userLists
                .Select(l => new UserListModel { UserListId = l.UserListId.ToString(), Name = l.Name, CanAddItems = true, ItemCount = userListCounts.TryGetValue(l.UserListId, out var listCount) ? listCount : 0 })
                .Prepend(new UserListModel { Name = "All", UserListId = "all", CanAddItems = true, ItemCount = totalCount });
            if (overdueCount > 0 || dueCount > 0)
            {
                var name = overdueCount > 0 && dueCount > 0
                    ? $"{overdueCount} overdue and {dueCount} due today"
                    : overdueCount > 0 ? $"{overdueCount} overdue" : $"{dueCount} due today";
                lists = lists.Prepend(new UserListModel { Name = name, UserListId = "due", CanAddItems = false, ItemCount = overdueCount + dueCount });
            }
            lists = lists.ToList();
            IEnumerable<UserItemModel> items;
            UserListModel selectedList;
            if (list != null)
            {
                if (list == "all")
                {
                    selectedList = lists.Single(l => l.UserListId == "all");
                    items = (await _userItemRepository.GetItemsAsync(user, null))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, null);
                }
                else if (list == "due")
                {
                    selectedList = lists.Single(l => l.UserListId == "due");
                    items = (await _userItemRepository.GetItemsAsync(user, null, new UserItemFilter { Overdue = true, DueToday = true }))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, -1);
                }
                else if (!int.TryParse(list, out var listId))
                    return BadRequest();
                else
                {
                    if (!userLists.Any(l => l.UserListId == listId))
                        return BadRequest();

                    selectedList = lists.Single(l => l.UserListId == list);
                    items = (await _userItemRepository.GetItemsAsync(user, userLists.FirstOrDefault(l => l.UserListId == listId)))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    await _userAccountRepository.SetLastSelectedUserListIdAsync(user, listId);
                }
            }
            else
            {
                selectedList = lists.FirstOrDefault(l => l.UserListId == user.LastSelectedUserListId?.ToString());
                if (selectedList == null)
                {
                    if (user.LastSelectedUserListId == -1)
                    {
                        selectedList = lists.Single(l => l.UserListId == "due");
                        items = (await _userItemRepository.GetItemsAsync(user, null, new UserItemFilter { Overdue = true, DueToday = true }))
                            .Select(i => new UserItemModel
                            {
                                UserItemId = i.UserItemId,
                                Description = i.Description,
                                Notes = i.Notes
                            }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    }
                    else
                    {
                        selectedList = lists.Single(l => l.UserListId == "all");
                        items = (await _userItemRepository.GetItemsAsync(user, null))
                            .Select(i => new UserItemModel
                            {
                                UserItemId = i.UserItemId,
                                Description = i.Description,
                                Notes = i.Notes
                            }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                    }
                }
                else
                {
                    items = (await _userItemRepository.GetItemsAsync(user, userLists.Single(l => l.UserListId.ToString() == selectedList.UserListId)))
                        .Select(i => new UserItemModel
                        {
                            UserItemId = i.UserItemId,
                            Description = i.Description,
                            Notes = i.Notes
                        }.WithDueDate(i.NextDueDate).WithRepeat(i.Repeat));
                }
            }

            return View(new IndexViewModel(HttpContext)
            {
                SelectedList = selectedList,
                Items = items,
                Lists = lists
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