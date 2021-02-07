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
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class ListsApiController : ControllerBase
    {
        private readonly ILogger<ListsApiController> _logger;
        private readonly IMediator _mediator;

        public ListsApiController(ILogger<ListsApiController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPut("~/api/v{version:apiVersion}/lists/{userListId}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Move(int userListId, MoveRequest moveRequest)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }
            
            if (!await _mediator.Send(new ReorderListRequest(User, userListId, moveRequest)))
            {
                _logger.LogInformation($"Could not move list {userListId} to {moveRequest.SortOrderPreviousListItemId}, returning not found");
                return NotFound();
            }

            return NoContent();
        }
    }
}