using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SmallLister.Web.Tests
{
    public class HomeTests
    {
        private readonly TestWebApplicationFactory<Startup> _factory = new TestWebApplicationFactory<Startup>();

        [Fact]
        public async Task Unauthorized_HomePage_Should_Redirect_To_Login()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var response = await client.GetAsync("/");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("http://localhost/signin?ReturnUrl=%2F");
        }

        [Fact]
        public async Task Get_HomePage()
        {
            var client = _factory.CreateAuthenticatedClient();
            var response = await client.GetAsync("/");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Logout").And.Contain("Nothing on this list");
        }
    }
}