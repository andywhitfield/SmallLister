using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Items;
using SmallLister.Web.Model.Request;

namespace SmallLister.Web.Controllers;

[Authorize]
public class ItemsController(ILogger<ItemsController> logger, IMediator mediator)
    : Controller
{
    [HttpPost("~/items/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromForm] AddOrUpdateItemRequest addModel)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        if (!await mediator.Send(new AddItemRequest(User, addModel)))
        {
            logger.LogInformation("Could not add item {Description} to list {List}, returning bad request", addModel.Description, addModel.List);
            return BadRequest();
        }

        return Redirect("~/");
    }

    [HttpGet("~/items/{userItemId}")]
    public async Task<IActionResult> Edit([FromRoute] int userItemId)
    {
        var response = await mediator.Send(new GetItemForEditRequest(User, userItemId));
        if (!response.IsValid || response.UserItem == null || response.SelectedList == null)
        {
            logger.LogInformation("Could not find item {UserItemId}, returning not found", userItemId);
            return NotFound();
        }

        return View(new EditViewModel(HttpContext, response.UserItem, response.Lists, response.SelectedList));
    }

    [HttpPost("~/items/{userItemId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int userItemId, [FromForm] AddOrUpdateItemRequest updateModel)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        IRequest<(bool, string)> request = (updateModel.Done ?? false)
            ? new MarkItemAsDoneRequest(User, userItemId)
            : new EditItemRequest(User, userItemId, updateModel);
        var (success, redirect) = await mediator.Send(request);
        if (!success)
        {
            logger.LogInformation("Could not update item {UserItemId}, returning not found", userItemId);
            return NotFound();
        }

        return Redirect(redirect);
    }

    [HttpPost("~/items/done/{userItemId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Done([FromRoute] int userItemId)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        var (success, redirect) = await mediator.Send(new MarkItemAsDoneRequest(User, userItemId));
        if (!success)
        {
            logger.LogInformation("Could not mark item {UserItemId} as done, returning not found", userItemId);
            return NotFound();
        }

        return Redirect(redirect);
    }

    [HttpPost("~/items/snooze/{userItemId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Snooze([FromRoute] int userItemId)
    {
        if (!ModelState.IsValid)
        {
            logger.LogInformation("Model state is invalid, returning bad request");
            return BadRequest();
        }

        if (!await mediator.Send(new SnoozeRequest(User, userItemId)))
        {
            logger.LogInformation("Could snooze {UserItemId}, returning not found", userItemId);
            return NotFound();
        }

        return Redirect("~/");
    }
}