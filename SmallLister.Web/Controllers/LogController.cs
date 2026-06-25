using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Log;

namespace SmallLister.Web.Controllers;

[Authorize]
public class LogController(IMediator mediator) : Controller
{
    [HttpGet("~/log")]
    public async Task<IActionResult> Index()
    {
        var response = await mediator.Send(new GetActionLogRequest(User));
        return View(new IndexViewModel(HttpContext,
            response.CurrentUndoAction != null, response.CurrentUndoAction?.Description,
            response.CurrentRedoAction != null, response.CurrentRedoAction?.Description,
            response.AllUndoRedoActions()));
    }
}
