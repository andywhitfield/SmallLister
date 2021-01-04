using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
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
    }
}