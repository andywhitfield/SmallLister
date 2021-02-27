using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly ILogger<HistoryController> _logger;
        private readonly IMediator _mediator;

        public HistoryController(ILogger<HistoryController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost("~/history/undo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undo()
        {
            if (!await _mediator.Send(new UndoRedoRequest(User, -1)))
            {
                _logger.LogInformation($"Could not perform undo action, returning bad request");
                return BadRequest();
            }

            return Redirect("~/");
        }

        [HttpPost("~/history/redo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Redo()
        {
            if (!await _mediator.Send(new UndoRedoRequest(User, 1)))
            {
                _logger.LogInformation($"Could not perform redo action, returning bad request");
                return BadRequest();
            }

            return Redirect("~/");
        }
    }
}