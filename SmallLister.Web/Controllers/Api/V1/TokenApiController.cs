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

namespace SmallLister.Web.Controllers.Api.V1
{
    [ApiController]
    public class TokenApiController : ControllerBase
    {
        private readonly ILogger<TokenApiController> _logger;
        private readonly IMediator _mediator;

        public TokenApiController(ILogger<TokenApiController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost("~/api/v1/token")]
        [ProducesResponseType(typeof(GetAccessTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAccessToken([FromHeader, Required] string authorization, [FromBody, Required] CreateAccessTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            if (!AuthenticationHeaderValue.TryParse(authorization, out var authHeader))
            {
                _logger.LogInformation($"Could not parse auth header [{authorization}]");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            if (authHeader.Scheme != "Basic")
            {
                _logger.LogInformation($"Auth header [{authorization}] is not Basic scheme");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            var accessTokenResponse = await _mediator.Send(new ValidateCredentialsAndCreateAccessTokenRequest(authHeader.Parameter, request.RefreshToken));
            if (accessTokenResponse == null)
            {
                _logger.LogInformation($"Could not create an access token for auth header [{authorization}]");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (accessTokenResponse.AccessToken == null)
                return BadRequest(accessTokenResponse);

            return Ok(accessTokenResponse);
        }
    }
}