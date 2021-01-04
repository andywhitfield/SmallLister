using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [ApiController]
    [Authorize]
    public class ItemsApiController : ControllerBase
    {
        private readonly ILogger<ItemsApiController> _logger;
        private readonly IMediator _mediator;

        public ItemsApiController(ILogger<ItemsApiController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPut("~/api/items/{userItemId}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Move(int userItemId, MoveRequest moveRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }
            
            if (!await _mediator.Send(new ReorderItemRequest(User, userItemId, moveRequest)))
            {
                _logger.LogInformation($"Could not move item {userItemId} to {moveRequest.SortOrderPreviousListItemId}, returning not found");
                return NotFound();
            }

            return NoContent();
        }
    }
}