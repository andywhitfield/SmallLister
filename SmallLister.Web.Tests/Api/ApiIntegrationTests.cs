using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Security;
using Xunit;

namespace SmallLister.Web.Tests.Api
{
    public class ApiIntegrationTests : IAsyncLifetime
    {
        private readonly IntegrationTestWebApplicationFactory _factory = new IntegrationTestWebApplicationFactory();

        private const string appKey = "test-appkey";
        private const string appSecret = "test-appsecret";
        private const string redirectUri = "http://test/redirect";

        public async Task InitializeAsync()
        {
            using var serviceScope = _factory.Services.CreateScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            context.Migrate();

            var userAccount = await context.UserAccounts.AddAsync(new UserAccount { AuthenticationUri = "http://test/user/1" });
            var userList = await context.UserLists.AddAsync(new UserList { Name = "Test list", UserAccount = userAccount.Entity });
            await context.UserItems.AddRangeAsync(
                new UserItem { Description = "Test item 1", UserAccount = userAccount.Entity },
                new UserItem { Description = "Test item 2", UserAccount = userAccount.Entity, UserList = userList.Entity, NextDueDate = DateTime.Today },
                new UserItem { Description = "Test item 3", UserAccount = userAccount.Entity, UserList = userList.Entity, NextDueDate = DateTime.Today.AddDays(-1), Repeat = ItemRepeat.Daily }
            );
            var (appSecretSalt, appSecretHash) = SaltHashHelper.CreateHash(appSecret);
            await context.ApiClients.AddAsync(new ApiClient { AppKey = appKey, AppSecretSalt = appSecretSalt, AppSecretHash = appSecretHash, RedirectUri = redirectUri, CreatedBy = userAccount.Entity, DisplayName = "test-api-client" });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Authorise_api_and_get_items()
        {
            using var client = _factory.CreateAuthenticatedClient(false);
            var responseContent = await RequestAuthoriseAsync(client);
            var refreshToken = await AuthoriseAsync(client, responseContent);
            var tokenResponse = await CreateTokenAsync(client, refreshToken);
            var listResponse = await GetListsAsync(client, tokenResponse);
            await GetListItemsAsync(client, tokenResponse, listResponse.Lists.Single().ListId);
        }

        private async Task<string> RequestAuthoriseAsync(HttpClient client)
        {
            var response = await client.GetAsync($"/api/v1/authorize?appkey={appKey}&redirect_uri={redirectUri}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("External application access");
            return responseContent;
        }

        private async Task<string> AuthoriseAsync(HttpClient client, string requestAuthContent)
        {
            var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(requestAuthContent, "/api/v1/authorize");
            var response = await client.PostAsync("/api/v1/authorize", new FormUrlEncodedContent(new[] {
                KeyValuePair.Create("__RequestVerificationToken", validationToken),
                KeyValuePair.Create("allowapiauth", "true"),
                KeyValuePair.Create("appkey", appKey),
                KeyValuePair.Create("redirecturi", redirectUri)
            }));
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.AbsoluteUri.Should().StartWith(redirectUri);
            var refreshTokenParam = "?refreshToken=";
            response.Headers.Location.Query.Should().StartWith(refreshTokenParam);
            return response.Headers.Location.Query.Substring(refreshTokenParam.Length);
        }

        private async Task<ApiTokenResponse> CreateTokenAsync(HttpClient client, string refreshToken)
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appKey}:{appSecret}"))}");
            var response = await client.PostAsJsonAsync("/api/v1/token", new { refreshToken });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tokenResponse = await response.Content.ReadFromJsonAsync<ApiTokenResponse>();
            tokenResponse.Should().NotBeNull();
            tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
            return tokenResponse;
        }

        private async Task<ApiListResponse> GetListsAsync(HttpClient client, ApiTokenResponse tokenResponse)
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            var response = await client.GetAsync("/api/v1/list");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var listResponse = await response.Content.ReadFromJsonAsync<ApiListResponse>();
            listResponse.Should().NotBeNull();
            listResponse.Lists.Should().HaveCount(1);
            listResponse.Lists.Single().Name.Should().Be("Test list");
            return listResponse;
        }

        private async Task GetListItemsAsync(HttpClient client, ApiTokenResponse tokenResponse, string listId)
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            var response = await client.GetAsync($"/api/v1/list/{listId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var listItemsResponse = await response.Content.ReadFromJsonAsync<ApiList>();
            listItemsResponse.Should().NotBeNull();
            listItemsResponse.Items.Should().HaveCount(2);
            listItemsResponse.Items.Select(x => x.Description).Should().BeEquivalentTo(new[] { "Test item 2", "Test item 3" });
        }

        public Task DisposeAsync()
        {
            _factory.Dispose();
            return Task.CompletedTask;
        }

        public class ApiTokenResponse
        {
            public string AccessToken { get; set; }
        }

        public class ApiListResponse
        {
            public IEnumerable<ApiList> Lists { get; set; }
        }

        public class ApiList
        {
            public string ListId { get; set; }
            public string Name { get; set; }
            public IEnumerable<ApiListItem> Items { get; set; }
        }
        public class ApiListItem
        {
            public string ItemId { get; set; }
            public string Description { get; set; }
            public DateTime? DueDate { get; set; }
        }
    }
}