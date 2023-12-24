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

namespace SmallLister.Web.Controllers.Api.V1;

[ApiVersion("1.0")]
[ApiController]
[Authorize("ApiJwt")]
public class ItemApiController(ILogger<ItemApiController> logger, IJwtService jwtService, IMediator mediator) : ControllerBase
{
    [HttpPost("~/api/v{version:apiVersion}/item")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddItem([Required] AddItemRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        var user = await jwtService.GetUserAccountAsync(User);
        if (user == null)
        {
            logger.LogInformation($"Could not get user account from token {User.Identity?.Name}, returning unauthorized");
            return Unauthorized();
        }

        var created = await mediator.Send(new AddItemApiRequest(user, model));
        if (!created)
        {
            logger.LogWarning($"Item [{model.Description}] could not be added to list [{model.ListId}]");
            return BadRequest();
        }

        return Ok();
    }

    [HttpDelete("~/api/v{version:apiVersion}/item")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteItem([Required] AddItemRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        var user = await jwtService.GetUserAccountAsync(User);
        if (user == null)
        {
            logger.LogInformation($"Could not get user account from token {User.Identity?.Name}, returning unauthorized");
            return Unauthorized();
        }

        var deleted = await mediator.Send(new DeleteItemApiRequest(user, model));
        if (!deleted)
            return BadRequest();

        return Ok();
    }
}