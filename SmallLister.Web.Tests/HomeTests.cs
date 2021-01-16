using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SmallLister.Web.Tests
{
    public class HomeTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private readonly TestWebApplicationFactory<Startup> _factory;

        public HomeTests(TestWebApplicationFactory<Startup> factory) => _factory = factory;

        [Fact]
        public async Task Unauthorized_HomePage_Should_Redirect_To_Login()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var response = await client.GetAsync("/");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.Should().Be("http://localhost/signin?ReturnUrl=%2F");
        }
    }
}