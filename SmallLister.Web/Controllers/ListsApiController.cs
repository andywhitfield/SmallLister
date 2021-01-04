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
    public class ListsApiController : ControllerBase
    {
        private readonly ILogger<ListsApiController> _logger;
        private readonly IMediator _mediator;

        public ListsApiController(ILogger<ListsApiController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPut("~/api/lists/{userListId}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Move(int userListId, MoveRequest moveRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            
            if (!await _mediator.Send(new ReorderListRequest(User, userListId, moveRequest)))
                return NotFound();

            return NoContent();
        }
    }
}