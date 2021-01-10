using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse.Api;
using SmallLister.Web.Model.Api;

namespace SmallLister.Web.Controllers.Api.V1
{
    [ApiController]
    [Authorize("ApiJwt")]
    public class ItemApiController : ControllerBase
    {
        private readonly ILogger<ItemApiController> _logger;
        private readonly IJwtService _jwtService;
        private readonly IMediator _mediator;

        public ItemApiController(ILogger<ItemApiController> logger, IJwtService jwtService, IMediator mediator)
        {
            _logger = logger;
            _jwtService = jwtService;
            _mediator = mediator;
        }

        [HttpPost("~/api/v1/item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddItem([Required] AddItemRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }
            
            var user = await _jwtService.GetUserAccountAsync(User);
            if (user == null)
            {
                _logger.LogInformation($"Could not get user account from token {User.Identity.Name}, returning unauthorized");
                return Unauthorized();
            }

            var created = await _mediator.Send(new AddItemApiRequest(user, model));
            if (!created)
                return BadRequest();

            return Ok();
        }
    }
}