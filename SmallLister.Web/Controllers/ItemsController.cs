using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
                return BadRequest();
            
            addModel.User = User;
            var added = await _mediator.Send(addModel);
            if (!added)
                return BadRequest();
            return Redirect("~/");
        }
    }
}