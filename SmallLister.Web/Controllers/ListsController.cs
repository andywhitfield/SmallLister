using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Model.Lists;

namespace SmallLister.Web.Controllers
{
    public class ListsController : Controller
    {
        private readonly ILogger<ListsController> _logger;
        private readonly IUserAccountRepository _userAccountRepository;

        public ListsController(ILogger<ListsController> logger,
            IUserAccountRepository userAccountRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
        }

        [Authorize]
        [HttpGet("~/lists")]
        public IActionResult Index() => View(new IndexViewModel(HttpContext));
    }
}