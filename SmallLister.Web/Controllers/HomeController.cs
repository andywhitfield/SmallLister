using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Home;

namespace SmallLister.Web.Controllers;

public class HomeController(IMediator mediator) : Controller
{
    [Authorize]
    [HttpGet("~/")]
    public async Task<IActionResult> Index([FromQuery] string list, [FromQuery] ItemSortOrder? sort, [FromQuery] int? pageNumber)
    {
        var response = await mediator.Send(new GetListItemsRequest(User, list, sort, pageNumber));
        if (!response.IsValid)
            return BadRequest();

        return View(new IndexViewModel(HttpContext, response.DueAndOverdueCount, response.Lists, response.SelectedList,
            response.Items, response.Pagination, response.UndoAction, response.RedoAction));
    }

    public IActionResult Error() => View(new ErrorViewModel(HttpContext));

    [HttpGet("~/signin")]
    public IActionResult Signin([FromQuery] string? returnUrl) => View("Login", new LoginViewModel(HttpContext, returnUrl));

    [HttpPost("~/signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signin([FromForm] string? returnUrl, [FromForm, Required] string email)
    {
        if (!ModelState.IsValid)
            return View("Login", new LoginViewModel(HttpContext, returnUrl));

        var response = await mediator.Send(new SigninRequest(email));
        return View("LoginVerify", new LoginVerifyViewModel(HttpContext, returnUrl, email,
            response.IsReturningUser, response.VerifyOptions));
    }

    [HttpPost("~/signin/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SigninVerify(
        [FromForm] string? returnUrl,
        [FromForm, Required] string email,
        [FromForm, Required] string verifyOptions,
        [FromForm, Required] string verifyResponse)
    {
        if (!ModelState.IsValid)
            return Redirect("~/signin");

        var isValid = await mediator.Send(new SigninVerifyRequest(HttpContext, email, verifyOptions, verifyResponse));
        if (isValid)
        {
            var redirectUri = "~/";
            if (!string.IsNullOrEmpty(returnUrl) && Uri.TryCreate(returnUrl, UriKind.Relative, out var uri))
                redirectUri = uri.ToString();

            return Redirect(redirectUri);
        }
        
        return Redirect("~/signin");
    }

    [HttpGet("~/signout"), HttpPost("~/signout")]
    public async Task<IActionResult> Signout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("~/");
    }
}