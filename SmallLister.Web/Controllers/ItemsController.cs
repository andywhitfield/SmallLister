using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ILogger<ItemsController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        private readonly IUserItemRepository _userItemRepository;

        public ItemsController(ILogger<ItemsController> logger,
            IUserAccountRepository userAccountRepository, IUserListRepository userListRepository, IUserItemRepository userItemRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
            _userItemRepository = userItemRepository;
        }

        [HttpPost("~/items/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] AddOrUpdateItemRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userAccountRepository.GetUserAccountAsync(User);
            UserList list = null;
            if (addModel.List.HasValue)
            {
                list = await _userListRepository.GetListAsync(user, addModel.List.Value);
                if (list == null)
                    return BadRequest();
            }

            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(addModel.Due))
            {
                if (!DateTime.TryParseExact(addModel.Due, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var due))
                    return BadRequest();
                dueDate = due.Date;
            }

            _logger.LogInformation($"Adding item to list {list?.UserListId} [{list?.Name}]: {addModel.Description}; due={dueDate}; repeat={addModel.Repeat}; notes={addModel.Notes}");
            await _userItemRepository.AddItemAsync(user, list, addModel.Description?.Trim(), addModel.Notes?.Trim(), dueDate, addModel.Repeat);
            return Redirect("~/");
        }
    }
}