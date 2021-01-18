using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Controllers
{
    public class FeedController : Controller
    {
        private readonly ILogger<FeedController> _logger;
        private readonly IMediator _mediator;

        public FeedController(ILogger<FeedController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("~/feed/{feedIdentifier}")]
        public async Task<IActionResult> Index([FromRoute, Required] string feedIdentifier)
        {
            var response = await _mediator.Send(new GetFeedRequest($"{Request.Scheme}://{Request.Host}", feedIdentifier));
            if (response == null)
                return NotFound();
            return Content(response, "application/xml", Encoding.UTF8);
        }
    }
}