using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse.Api;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Controllers.Api.V1
{
    [ApiController]
    [Authorize("ApiJwt")]
    public class ListApiController : ControllerBase
    {
        private readonly ILogger<ListApiController> _logger;
        private readonly IJwtService _jwtService;
        private readonly IMediator _mediator;

        public ListApiController(ILogger<ListApiController> logger, IJwtService jwtService, IMediator mediator)
        {
            _logger = logger;
            _jwtService = jwtService;
            _mediator = mediator;
        }

        [HttpGet("~/api/v1/list")]
        [ProducesResponseType(typeof(GetAllListsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllLists()
        {
            var user = await _jwtService.GetUserAccountAsync(User);
            if (user == null)
            {
                _logger.LogInformation($"Could not get user account from token {User.Identity.Name}, returning unauthorized");
                return Unauthorized();
            }

            return Ok(await _mediator.Send(new GetAllListsRequest(user)));
        }

        [HttpGet("~/api/v1/list/{listId}")]
        [ProducesResponseType(typeof(GetListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetList([Required] string listId)
        {
            var user = await _jwtService.GetUserAccountAsync(User);
            if (user == null)
            {
                _logger.LogInformation($"Could not get user account from token {User.Identity.Name}, returning unauthorized");
                return Unauthorized();
            }

            var response = await _mediator.Send(new GetListRequest(user, listId));
            if (response == null)
                return NotFound();

            return Ok(response);
        }
    }
}