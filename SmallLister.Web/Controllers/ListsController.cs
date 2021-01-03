using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Model;
using SmallLister.Web.Model.Lists;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class ListsController : Controller
    {
        private readonly ILogger<ListsController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;

        public ListsController(ILogger<ListsController> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }

        [HttpGet("~/lists")]
        public async Task<IActionResult> Index()
        {
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var lists = await _userListRepository.GetListsAsync(user);
            return View(new IndexViewModel(HttpContext)
            {
                Lists = lists.Select(l => new UserListModel
                {
                    UserListId = l.UserListId.ToString(),
                    Name = l.Name
                })
            });
        }

        [HttpPost("~/lists")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] AddOrUpdateListRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            _logger.LogInformation($"Adding new list: {addModel.Name}");
            await _userListRepository.AddListAsync(user, addModel.Name?.Trim());
            return Redirect("~/lists");
        }

        [HttpPost("~/lists/{userListId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromRoute] int userListId, [FromForm] AddOrUpdateListRequest updateModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var list = await _userListRepository.GetListAsync(user, userListId);
            if (list == null)
                return NotFound();
            _logger.LogInformation($"Updating name of list {list.UserListId} [{list.Name}] to [{updateModel.Name}]");
            list.Name = updateModel.Name;
            await _userListRepository.SaveAsync(list);
            return Redirect("~/lists");
        }

        [HttpPost("~/lists/delete/{userListId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] int userListId)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            var list = await _userListRepository.GetListAsync(user, userListId);
            if (list == null)
                return NotFound();
            _logger.LogInformation($"Deleting list {list.UserListId} [{list.Name}]");
            list.DeletedDateTime = DateTime.UtcNow;
            await _userListRepository.SaveAsync(list);
            return Redirect("~/lists");
        }
    }
}