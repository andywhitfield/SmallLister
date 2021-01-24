using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Api;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers.Api.V1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class AuthorizeController : Controller
    {
        private readonly ILogger<AuthorizeController> _logger;
        private readonly IMediator _mediator;

        public AuthorizeController(ILogger<AuthorizeController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("~/api/v{version:apiVersion}/authorize")]
        public async Task<IActionResult> Authorize(
            [Required, FromQuery(Name = "appkey")] string appKey,
            [Required, FromQuery(Name = "redirect_uri")] string redirectUri)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            var response = await _mediator.Send(new GetApiAuthorizeRequest(appKey, redirectUri));
            if (!response.IsValid)
            {
                _logger.LogInformation($"Could not complete api authorization request for app key {appKey}, returning not found");
                return NotFound();
            }

            return View(new ApiAuthorizeModel(HttpContext, response.ApplicationName, response.AppKey, response.RedirectUri));
        }

        [HttpPost("~/api/v{version:apiVersion}/authorize")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuthorizeAsync([FromForm] AuthorizeRequest model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            var redirectResponse = await _mediator.Send(new ApiAuthorizeRequest(User, model));
            if (redirectResponse == null)
            {
                _logger.LogInformation($"Could not complete api authorization for app key {model.AppKey}, returning not found");
                return NotFound();
            }

            return Redirect(redirectResponse);
        }        
    }
}