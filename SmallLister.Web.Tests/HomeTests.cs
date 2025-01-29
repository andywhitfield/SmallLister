using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmallLister.Web.Tests;

[TestClass]
public sealed class HomeTests
{
    private readonly TestWebApplicationFactory<Startup> _factory = new();

    [TestMethod]
    public async Task Unauthorized_HomePage_Should_Redirect_To_Login()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().Be("http://localhost/signin?ReturnUrl=%2F");
    }

    [TestMethod]
    public async Task Get_HomePage()
    {
        var client = _factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout").And.Contain("Nothing on this list");
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();
}