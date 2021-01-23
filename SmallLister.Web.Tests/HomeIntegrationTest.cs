using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SmallLister.Data;
using SmallLister.Model;
using Xunit;

namespace SmallLister.Web.Tests
{
    public class HomeIntegrationTest : IAsyncLifetime
    {
        private readonly IntegrationTestWebApplicationFactory _factory = new IntegrationTestWebApplicationFactory();

        public async Task InitializeAsync()
        {
            using (var serviceScope = _factory.Services.CreateScope())
            {
                using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
                context.Migrate();
                var userAccount = await context.UserAccounts.AddAsync(new UserAccount { AuthenticationUri = "http://test/user/1" });
                var userList = await context.UserLists.AddAsync(new UserList { Name = "Test list", UserAccount = userAccount.Entity });
                await context.UserItems.AddRangeAsync(
                    new UserItem { Description = "Test item 1", UserAccount = userAccount.Entity },
                    new UserItem { Description = "Test item 2", UserAccount = userAccount.Entity, UserList = userList.Entity, NextDueDate = DateTime.Today },
                    new UserItem { Description = "Test item 3", UserAccount = userAccount.Entity, NextDueDate = DateTime.Today.AddDays(-1), Repeat = ItemRepeat.Daily }
                );
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task Should_display_expected_items()
        {
            using var client = _factory.CreateAuthenticatedClient();
            var response = await client.GetAsync("/");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Logout")
                .And.Contain("1 overdue and 1 due today (2)")
                .And.Contain("All (3)")
                .And.Contain("Test list (1)")
                .And.Contain("Test item 1")
                .And.Contain("Test item 2")
                .And.Contain("Test item 3")
                .And.Contain("Due yesterday")
                .And.Contain("Repeats every day")
                .And.Contain("Due today");
        }

        public Task DisposeAsync()
        {
            _factory.Dispose();
            return Task.CompletedTask;
        }
    }

    public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<SqliteDataContext> _options;

        public IntegrationTestWebApplicationFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(_connection).Options;
        }

        protected override IHostBuilder CreateHostBuilder() => Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(x => x.UseStartup<Startup>().UseTestServer().ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Scoped<SqliteDataContext>(_ => new SqliteDataContext(_options)));
                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestStubAuthHandler>("Test", null);
            }));

        public HttpClient CreateAuthenticatedClient()
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            return client;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _connection.Dispose();
        }
    }
}