using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Lists;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class ListsController : Controller
    {
        private readonly ILogger<ListsController> _logger;
        private readonly IMediator _mediator;

        public ListsController(ILogger<ListsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("~/lists")]
        public async Task<IActionResult> Index() => View(new IndexViewModel(HttpContext)
        {
            Lists = await _mediator.Send(new GetListsRequest(User))
        });

        [HttpPost("~/lists")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] AddOrUpdateListRequest addModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            await _mediator.Send(new AddListRequest(User, addModel));
            return Redirect("~/lists");
        }

        [HttpPost("~/lists/{userListId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromRoute] int userListId, [FromForm] AddOrUpdateListRequest updateModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!await _mediator.Send(new UpdateListRequest(User, userListId, updateModel)))
                return NotFound();

            return Redirect("~/lists");
        }

        [HttpPost("~/lists/delete/{userListId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] int userListId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!await _mediator.Send(new DeleteListRequest(User, userListId)))
                return NotFound();

            return Redirect("~/lists");
        }
    }
}