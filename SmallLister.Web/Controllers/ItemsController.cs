using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Items;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ILogger<ItemsController> _logger;
        private readonly IMediator _mediator;

        public ItemsController(ILogger<ItemsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost("~/items/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] AddOrUpdateItemRequest addModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }
            
            if (!await _mediator.Send(new AddItemRequest(User, addModel)))
            {
                _logger.LogInformation($"Could not add item {addModel.Description} to list {addModel.List}, returning bad request");
                return BadRequest();
            }

            return Redirect("~/");
        }

        [HttpGet("~/items/{userItemId}")]
        public async Task<IActionResult> Edit([FromRoute] int userItemId)
        {
            var response = await _mediator.Send(new GetItemForEditRequest(User, userItemId));
            if (!response.IsValid || response.UserItem == null || response.SelectedList == null)
            {
                _logger.LogInformation($"Could not find item {userItemId}, returning not found");
                return NotFound();
            }

            return View(new EditViewModel(HttpContext, response.UserItem, response.Lists, response.SelectedList));
        }

        [HttpPost("~/items/{userItemId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int userItemId, [FromForm] AddOrUpdateItemRequest updateModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            IRequest<bool> request = (updateModel.Done ?? false)
                ? new MarkItemAsDoneRequest(User, userItemId)
                : new EditItemRequest(User, userItemId, updateModel);            
            if (!await _mediator.Send(request))
            {
                _logger.LogInformation($"Could not update item {userItemId}, returning not found");
                return NotFound();
            }

            return Redirect("~/");
        }

        [HttpPost("~/items/done/{userItemId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Done([FromRoute] int userItemId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }
            
            if (!await _mediator.Send(new MarkItemAsDoneRequest(User, userItemId)))
            {
                _logger.LogInformation($"Could not mark item {userItemId} as done, returning not found");
                return NotFound();
            }

            return Redirect("~/");
        }

        [HttpPost("~/items/snooze/{userItemId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Snooze([FromRoute] int userItemId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }
            
            if (!await _mediator.Send(new SnoozeRequest(User, userItemId)))
            {
                _logger.LogInformation($"Could snooze {userItemId}, returning not found");
                return NotFound();
            }

            return Redirect("~/");
        }
    }
}