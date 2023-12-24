using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Model;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse.Api;
using SmallLister.Web.Model.Api;

namespace SmallLister.Web.Controllers.Api.V1;

[ApiVersion("1.0")]
[ApiController]
[Authorize("ApiJwt")]
public class WebhookApiController(ILogger<WebhookApiController> logger, IJwtService jwtService, IMediator mediator) : ControllerBase
{
    [HttpPost("~/api/v{version:apiVersion}/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterWebhook([Required] AddWebhookRequestModel model)
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

        if (!await mediator.Send(new AddWebhookRequest(user, model)))
            return Conflict();

        return Ok();
    }

    [HttpDelete("~/api/v{version:apiVersion}/webhook/{webhookType}")]
    public async Task<IActionResult> DeleteWebhook([Required] WebhookType webhookType)
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

        await mediator.Send(new DeleteWebhookRequest(user, webhookType));
        return Ok();
    }
}
