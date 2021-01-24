using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Find;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class FindController : Controller
    {
        private readonly IMediator _mediator;

        public FindController(IMediator mediator) => _mediator = mediator;

        [HttpGet("~/find")]
        public async Task<IActionResult> Find([FromQuery] string q)
        {
            var response = await _mediator.Send(new FindItemRequest(User, q));
            return View(new FindViewModel(HttpContext, q, response.Lists, response.Items));
        }
    }
}