using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Controllers;

[Authorize]
public class HistoryController(ILogger<HistoryController> logger, IMediator mediator)
    : Controller
{
    [HttpPost("~/history/undo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Undo([FromForm] string? returnUrl)
    {
        if (!await mediator.Send(new UndoRedoRequest(User, -1)))
        {
            logger.LogInformation("Could not perform undo action, returning bad request");
            return BadRequest();
        }

        return RedirectTo(returnUrl);
    }

    [HttpPost("~/history/redo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Redo([FromForm] string? returnUrl)
    {
        if (!await mediator.Send(new UndoRedoRequest(User, 1)))
        {
            logger.LogInformation("Could not perform redo action, returning bad request");
            return BadRequest();
        }

        return RedirectTo(returnUrl);
    }

    private RedirectResult RedirectTo(string? returnUrl)
    {
        var redirectUri = "~/";
        if (!string.IsNullOrEmpty(returnUrl) && Uri.TryCreate(returnUrl, UriKind.Relative, out var uri))
            redirectUri = uri.ToString();
        return Redirect(redirectUri);
    }
}