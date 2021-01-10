using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Profile;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IMediator _mediator;

        public ProfileController(ILogger<ProfileController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("~/profile")]
        public async Task<IActionResult> Index()
        {
            var response = await _mediator.Send(new GetProfileRequest(User));
            return View(new IndexViewModel(HttpContext, response.ExternalApiAccessList, response.ExternalApiClients));
        }

        [HttpPost("~/profile/externalclient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExternalClient([FromForm] AddExternalClientRequest model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            var response = await _mediator.Send(new CreateExternalClientRequest(User, model));
            return View(new CreatedExternalClientViewModel(HttpContext, response.DisplayName, response.RedirectUri, response.AppKey, response.AppSecret));
        }

        [HttpPost("~/profile/externalclient/update/{apiClientId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExternalClient([FromRoute, Required] int apiClientId, [FromForm, Required] string state)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            var updated = await _mediator.Send(new UpdateExternalClientRequest(User, apiClientId, state));
            if (!updated)
                return BadRequest();

            return Redirect("~/profile");
        }

        [HttpPost("~/profile/externalapp/revoke/{userAccountApiAccessId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeExternalApiAccess([FromRoute, Required] int userAccountApiAccessId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Model state is invalid, returning bad request");
                return BadRequest();
            }

            var revoked = await _mediator.Send(new RevokeExternalApiAccessRequest(User, userAccountApiAccessId));
            if (!revoked)
                return BadRequest();

            return Redirect("~/profile");
        }
    }
}