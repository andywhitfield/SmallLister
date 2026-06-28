using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Web.Tests;

[TestClass]
public class LogIntegrationTest
{
    private readonly IntegrationTestWebApplicationFactory _factory = new();
    private UserList? _userList;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.Migrate();
        var userAccount = await context.UserAccounts.AddAsync(new() { Email = "test-user-1" });
        var userList = await context.UserLists.AddAsync(new() { Name = "Test list", UserAccount = userAccount.Entity });
        await context.SaveChangesAsync();
        _userList = userList.Entity;
    }

    [TestMethod]
    public async Task Should_show_correct_log_for_item()
    {
        Fixture fixture = new();
        using var client = _factory.CreateAuthenticatedClient();
        using var response = await client.GetAsync("/log");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.NotContain("Undo")
            .And.NotContain("Redo");

        var taskDescription = fixture.Create<string>();

        responseContent = await AddItemAsync(client, taskDescription);
        responseContent
            .Should().Contain(taskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");

        await AssertItemHasDescriptionAsync(taskDescription);

        var updatedTaskDescription = fixture.Create<string>();
        responseContent = await UpdateItemAsync(client, taskDescription, updatedTaskDescription);
        responseContent
            .Should().Contain(taskDescription)
            .And.Contain(updatedTaskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");

        await AssertItemHasDescriptionAsync(updatedTaskDescription);

        responseContent = await UndoAsync(client, responseContent);
        responseContent
            .Should().Contain(taskDescription)
            .And.Contain(updatedTaskDescription)
            .And.Contain("Undo")
            .And.Contain("Redo");

        await AssertItemHasDescriptionAsync(taskDescription);

        responseContent = await RedoAsync(client, responseContent);
        responseContent
            .Should().Contain(taskDescription)
            .And.Contain(updatedTaskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");

        await AssertItemHasDescriptionAsync(updatedTaskDescription);
    }

    private async Task AssertItemHasDescriptionAsync(string description)
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var userItem = await context.UserItems.SingleOrDefaultAsync();
        userItem.Should().NotBeNull();
        userItem.Description.Should().Be(description);
    }

    private async Task<string> AddItemAsync(HttpClient client, string description)
    {
        using var homeResponse = await client.GetAsync("/");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var homePage = await homeResponse.Content.ReadAsStringAsync();
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(homePage, "/items/add");

        using var postResponse = await client.PostAsync("/items/add", new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", _userList?.UserListId.ToString() ?? ""),
            KeyValuePair.Create("description", description)
        ]));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var logResponse = await client.GetAsync("/log");
        logResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return await logResponse.Content.ReadAsStringAsync();
    }

    private async Task<string> UpdateItemAsync(HttpClient client, string currentDescription, string updatedDescription)
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var userItem = await context.UserItems.FirstOrDefaultAsync(u => u.Description == currentDescription);
        userItem.Should().NotBeNull();

        using var itemResponse = await client.GetAsync($"/items/{userItem.UserItemId}");
        itemResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var itemPage = await itemResponse.Content.ReadAsStringAsync();
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(itemPage, $"/items/{userItem.UserItemId}");

        using var postResponse = await client.PostAsync($"/items/{userItem.UserItemId}", new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", _userList?.UserListId.ToString() ?? ""),
            KeyValuePair.Create("description", updatedDescription)
        ]));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var logResponse = await client.GetAsync("/log");
        logResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return await logResponse.Content.ReadAsStringAsync();
    }

    private static Task<string> UndoAsync(HttpClient client, string page)
        => UndoRedoAsync(client, page, "/history/undo");

    private static Task<string> RedoAsync(HttpClient client, string page)
        => UndoRedoAsync(client, page, "/history/redo");

    private static async Task<string> UndoRedoAsync(HttpClient client, string page, string action)
    {
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, action);
        using var response = await client.PostAsync(action, new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken)
        ]));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var logResponse = await client.GetAsync("/log");
        logResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return await logResponse.Content.ReadAsStringAsync();
    }

    [TestCleanup]
    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}