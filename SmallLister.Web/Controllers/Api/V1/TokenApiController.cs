using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Request;
using SmallLister.Web.Model.Response;

namespace SmallLister.Web.Controllers.Api.V1;

[ApiVersion("1.0")]
[ApiController]
public class TokenApiController(ILogger<TokenApiController> logger, IMediator mediator) : ControllerBase
{
    [HttpPost("~/api/v{version:apiVersion}/token")]
    [ProducesResponseType(typeof(GetAccessTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAccessToken([FromHeader, Required] string authorization, [FromBody, Required] CreateAccessTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        if (!AuthenticationHeaderValue.TryParse(authorization, out var authHeader))
        {
            logger.LogInformation($"Could not parse auth header [{authorization}]");
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
        if (authHeader.Scheme != "Basic")
        {
            logger.LogInformation($"Auth header [{authorization}] is not Basic scheme");
            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        var accessTokenResponse = await mediator.Send(new ValidateCredentialsAndCreateAccessTokenRequest(authHeader.Parameter ?? "", request.RefreshToken));
        if (accessTokenResponse == null)
        {
            logger.LogInformation($"Could not create an access token for auth header [{authorization}]");
            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        if (accessTokenResponse.AccessToken == null)
            return BadRequest(accessTokenResponse);

        return Ok(accessTokenResponse);
    }
}