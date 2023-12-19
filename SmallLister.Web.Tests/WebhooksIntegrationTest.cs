using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallLister.Data;
using SmallLister.Model;
using Xunit;

namespace SmallLister.Web.Tests;

public class WebhooksIntegrationTest : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory = new();
    private UserList _userList;

    public async Task InitializeAsync()
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.Migrate();
        var userAccount = context.UserAccounts.Add(new() { Email = "test-user-1" });
        var userList = context.UserLists.Add(new() { Name = "Test list", UserAccount = userAccount.Entity });
        context.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = WebhookType.ListItemChange, Webhook = new("http://localhost/webhook") });
        await context.SaveChangesAsync();
        _userList = userList.Entity;
    }

    [Fact]
    public async Task Adding_item_to_list_should_add_to_webhook_queue()
    {
        Fixture fixture = new();
        var firstTaskDescription = fixture.Create<string>();
        var secondTaskDescription = fixture.Create<string>();
        using var client = _factory.CreateAuthenticatedClient();
        using var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.Contain("All (0)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (0)");
        (await GetUserItemWebhookQueueAsync()).Should().BeEmpty();

        responseContent = await AddItemAsync(client, responseContent, firstTaskDescription);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.Contain(firstTaskDescription);
        (await GetUserItemWebhookQueueAsync()).Should().HaveCount(1)
            .And.Satisfy(uiwq => uiwq.EventType == WebhookEventType.New && uiwq.UserItem.Description == firstTaskDescription);

        responseContent = await AddItemAsync(client, responseContent, secondTaskDescription);
        responseContent.Should().Contain("All (2)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (2)")
            .And.Contain(firstTaskDescription)
            .And.Contain(secondTaskDescription);

        var userItemWebhookQueue = await GetUserItemWebhookQueueAsync();
        userItemWebhookQueue.Should().HaveCount(2);
        userItemWebhookQueue.First().EventType.Should().Be(WebhookEventType.New);
        userItemWebhookQueue.First().UserItem.Description.Should().Be(firstTaskDescription);
        userItemWebhookQueue.Last().EventType.Should().Be(WebhookEventType.New);
        userItemWebhookQueue.Last().UserItem.Description.Should().Be(secondTaskDescription);

        responseContent = await MarkItemDoneAsync(client, responseContent, firstTaskDescription);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.Contain(secondTaskDescription);

        userItemWebhookQueue = await GetUserItemWebhookQueueAsync();
        userItemWebhookQueue.Should().HaveCount(3);
        userItemWebhookQueue.ElementAt(0).EventType.Should().Be(WebhookEventType.New);
        userItemWebhookQueue.ElementAt(0).UserItem.Description.Should().Be(firstTaskDescription);
        userItemWebhookQueue.ElementAt(1).EventType.Should().Be(WebhookEventType.New);
        userItemWebhookQueue.ElementAt(1).UserItem.Description.Should().Be(secondTaskDescription);
        userItemWebhookQueue.ElementAt(2).EventType.Should().Be(WebhookEventType.Modify);
        userItemWebhookQueue.ElementAt(2).UserItem.Description.Should().Be(firstTaskDescription);
    }

    private async Task<string> AddItemAsync(HttpClient client, string page, string description)
    {
        var addAction = "/items/add";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, addAction);

        using var response = await client.PostAsync(addAction, new FormUrlEncodedContent(new[] {
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", _userList.UserListId.ToString()),
            KeyValuePair.Create("description", description)
        }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> MarkItemDoneAsync(HttpClient client, string page, string description)
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var userItem = await context.UserItems.FirstOrDefaultAsync(u => u.Description == description);
        userItem.Should().NotBeNull();

        var userAccount = context.UserAccounts.Add(new() { Email = "test-user-1" });
        var userList = context.UserLists.Add(new() { Name = "Test list", UserAccount = userAccount.Entity });
        await context.SaveChangesAsync();
        _userList = userList.Entity;

        var doneAction = $"/items/done/{userItem.UserItemId}";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, doneAction);

        using var response = await client.PostAsync(doneAction, new FormUrlEncodedContent(new[] {
            KeyValuePair.Create("__RequestVerificationToken", validationToken)
        }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<IEnumerable<UserItemWebhookQueue>> GetUserItemWebhookQueueAsync()
    {
        await using var serviceScope = _factory.Services.CreateAsyncScope();
        var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        return await context.UserItemWebhookQueue.Include(x => x.UserItem).ToListAsync();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}