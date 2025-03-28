using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Web.Tests;

[TestClass]
public class HistoryIntegrationTest
{
    private readonly IntegrationTestWebApplicationFactory _factory = new IntegrationTestWebApplicationFactory();
    private UserList? _userList;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.Migrate();
        var userAccount = await context.UserAccounts.AddAsync(new UserAccount { Email = "test-user-1" });
        var userList = await context.UserLists.AddAsync(new UserList { Name = "Test list", UserAccount = userAccount.Entity });
        await context.SaveChangesAsync();
        _userList = userList.Entity;
    }

    [TestMethod]
    public async Task Should_add_two_items_then_mark_one_as_done_then_undo_all_then_redo_all()
    {
        var fixture = new Fixture();
        var firstTaskDescription = fixture.Create<string>();
        var secondTaskDescription = fixture.Create<string>();
        using var client = _factory.CreateAuthenticatedClient();
        using var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.Contain("All (0)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (0)")
            .And.NotContain("Undo")
            .And.NotContain("Redo");

        responseContent = await AddItemAsync(client, responseContent, firstTaskDescription);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.Contain(firstTaskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");

        responseContent = await AddItemAsync(client, responseContent, secondTaskDescription);
        responseContent.Should().Contain("All (2)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (2)")
            .And.Contain(firstTaskDescription)
            .And.Contain(secondTaskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");

        responseContent = await MarkItemDoneAsync(client, responseContent, firstTaskDescription);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.Contain(secondTaskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");

        responseContent = await UndoAsync(client, responseContent);
        responseContent.Should().Contain("All (2)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (2)")
            .And.Contain(firstTaskDescription)
            .And.Contain(secondTaskDescription)
            .And.Contain("Undo")
            .And.Contain("Redo");

        responseContent = await UndoAsync(client, responseContent);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.Contain(firstTaskDescription)
            .And.NotContain(secondTaskDescription)
            .And.Contain("Undo")
            .And.Contain("Redo");

        responseContent = await UndoAsync(client, responseContent);
        responseContent.Should().Contain("All (0)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (0)")
            .And.NotContain(firstTaskDescription)
            .And.NotContain(secondTaskDescription)
            .And.NotContain("Undo")
            .And.Contain("Redo");

        responseContent = await RedoAsync(client, responseContent);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.Contain(firstTaskDescription)
            .And.NotContain(secondTaskDescription)
            .And.Contain("Undo")
            .And.Contain("Redo");

        responseContent = await RedoAsync(client, responseContent);
        responseContent.Should().Contain("All (2)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (2)")
            .And.Contain(firstTaskDescription)
            .And.Contain(secondTaskDescription)
            .And.Contain("Undo")
            .And.Contain("Redo");

        responseContent = await RedoAsync(client, responseContent);
        responseContent.Should().Contain("All (1)")
            .And.Contain("All upcoming (0)")
            .And.Contain("Test list (1)")
            .And.NotContain(firstTaskDescription)
            .And.Contain(secondTaskDescription)
            .And.Contain("Undo")
            .And.NotContain("Redo");
    }

    private async Task<string> AddItemAsync(HttpClient client, string page, string description)
    {
        var addAction = "/items/add";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, addAction);

        using var response = await client.PostAsync(addAction, new FormUrlEncodedContent(new[] {
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", _userList?.UserListId.ToString() ?? ""),
            KeyValuePair.Create("description", description)
        }));
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Response: {await response.Content.ReadAsStringAsync()}");
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> MarkItemDoneAsync(HttpClient client, string page, string description)
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var userItem = await context.UserItems.FirstOrDefaultAsync(u => u.Description == description);
        userItem.Should().NotBeNull();

        var userAccount = await context.UserAccounts.AddAsync(new UserAccount { Email = "test-user-1" });
        var userList = await context.UserLists.AddAsync(new UserList { Name = "Test list", UserAccount = userAccount.Entity });
        await context.SaveChangesAsync();
        _userList = userList.Entity;

        var doneAction = $"/items/done/{userItem?.UserItemId}";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, doneAction);

        using var response = await client.PostAsync(doneAction, new FormUrlEncodedContent(new[] {
            KeyValuePair.Create("__RequestVerificationToken", validationToken)
        }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> UndoAsync(HttpClient client, string page)
    {
        const string undoAction = "/history/undo";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, undoAction);
        using var response = await client.PostAsync(undoAction, new FormUrlEncodedContent(new[] {
            KeyValuePair.Create("__RequestVerificationToken", validationToken)
        }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> RedoAsync(HttpClient client, string page)
    {
        const string redoAction = "/history/redo";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(page, redoAction);
        using var response = await client.PostAsync(redoAction, new FormUrlEncodedContent(new[] {
            KeyValuePair.Create("__RequestVerificationToken", validationToken)
        }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    [TestCleanup]
    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}