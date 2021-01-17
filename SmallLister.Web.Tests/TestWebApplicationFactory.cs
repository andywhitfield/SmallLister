using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Web.Tests
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private bool _addTestAuth;

        public Mock<IUserAccountRepository> UserAccountRepositoryMock { get; } = new Mock<IUserAccountRepository>();
        public Mock<IUserListRepository> UserListRepositoryMock { get; } = new Mock<IUserListRepository>();
        public Mock<IUserItemRepository> UserItemRepositoryMock { get; } = new Mock<IUserItemRepository>();

        protected override IHostBuilder CreateHostBuilder() => Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(x => x.UseStartup<Startup>().UseTestServer().ConfigureTestServices(services =>
            {
                var dataContext = services.FirstOrDefault(s => s.ServiceType == typeof(SqliteDataContext));
                if (dataContext != null)
                    services.Remove(dataContext);
                services.AddScoped<ISqliteDataContext>(_ => Mock.Of<ISqliteDataContext>());
                services.Replace(ServiceDescriptor.Scoped<IApiClientRepository>(_ => Mock.Of<IApiClientRepository>()));
                services.Replace(ServiceDescriptor.Scoped<IUserAccountRepository>(_ => UserAccountRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped<IUserAccountApiAccessRepository>(_ => Mock.Of<IUserAccountApiAccessRepository>()));
                services.Replace(ServiceDescriptor.Scoped<IUserAccountTokenRepository>(_ => Mock.Of<IUserAccountTokenRepository>()));
                services.Replace(ServiceDescriptor.Scoped<IUserItemRepository>(_ => UserItemRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped<IUserListRepository>(_ => UserListRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped<IUserFeedRepository>(_ => Mock.Of<IUserFeedRepository>()));
                if (_addTestAuth)
                    services
                        .AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestStubAuthHandler>("Test", null);
            }));

        public HttpClient CreateAuthenticatedClient()
        {
            _addTestAuth = true;
            UserAccountRepositoryMock.Setup(r => r.GetUserAccountAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new UserAccount());
            UserListRepositoryMock.Setup(r => r.GetListsAsync(It.IsAny<UserAccount>())).ReturnsAsync(new List<UserList>());
            UserItemRepositoryMock.Setup(r => r.GetItemsAsync(It.IsAny<UserAccount>(), It.IsAny<UserList>(), null, null, null)).ReturnsAsync((new List<UserItem>(), 1, 1));

            var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            return client;
        }
    }
}
