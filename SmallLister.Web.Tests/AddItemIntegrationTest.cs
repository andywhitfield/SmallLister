using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Web.Tests;

[TestClass]
public class AddItemIntegrationTest
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
        context.UserLists.Add(new() { Name = "Test list 1", UserAccount = userAccount.Entity });
        context.UserLists.Add(new() { Name = "Test list 2", UserAccount = userAccount.Entity });
        await context.SaveChangesAsync();
        _userList = context.UserLists.Single(x => x.Name == "Test list 1");
    }

    [TestMethod]
    public async Task Should_add_item()
    {
        using var client = _factory.CreateAuthenticatedClient();
        using var homeResponse = await client.GetAsync($"/?list={_userList!.UserListId}");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var homePage = await homeResponse.Content.ReadAsStringAsync();
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(homePage, "/items/add");

        using var postResponse = await client.PostAsync("/items/add", new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", _userList!.UserListId.ToString()),
            KeyValuePair.Create("description", "test item 1 on test list 1")
        ]));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await postResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test list 1 (1)", homePage);
        Assert.Contains("test item 1 on test list 1", homePage);
    }

    [TestMethod]
    public async Task Given_viewing_list1_Can_add_item_on_list_2()
    {
        UserList list2;
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            list2 = await context.UserLists.SingleAsync(x => x.Name == "Test list 2");
        }
        using var client = _factory.CreateAuthenticatedClient();
        using var homeResponse = await client.GetAsync($"/?list={_userList!.UserListId}");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var homePage = await homeResponse.Content.ReadAsStringAsync();
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(homePage, "/items/add");

        using var postResponse = await client.PostAsync("/items/add", new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", list2.UserListId.ToString()),
            KeyValuePair.Create("description", "test item 1 on test list 2")
        ]));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await postResponse.Content.ReadAsStringAsync();
        Assert.Contains("Test list 2 (1)", homePage);
        Assert.DoesNotContain("test item 1 on test list 2", homePage);

        // now switching to list 2 should be displayed
        using var homeResponseList2 = await client.GetAsync($"/?list={list2.UserListId}");
        homeResponseList2.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await homeResponseList2.Content.ReadAsStringAsync();
        Assert.Contains("Test list 2 (1)", homePage);
        Assert.Contains("test item 1 on test list 2", homePage);
    }

    [TestMethod]
    public async Task Should_add_item_on_new_list()
    {
        using var client = _factory.CreateAuthenticatedClient();
        using var homeResponse = await client.GetAsync($"/?list={_userList!.UserListId}");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var homePage = await homeResponse.Content.ReadAsStringAsync();
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(homePage, "/items/add");

        using var postResponse = await client.PostAsync("/items/add", new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", "new"),
            KeyValuePair.Create("newlist", "New test list"),
            KeyValuePair.Create("description", "test item 1 on new test list")
        ]));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await postResponse.Content.ReadAsStringAsync();
        Assert.Contains("New test list (1)", homePage);
        Assert.DoesNotContain("test item 1 on new test list", homePage);

        UserList? newList;
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            newList = await context.UserLists.SingleOrDefaultAsync(x => x.Name == "New test list");
        }
        Assert.IsNotNull(newList);

        // now switching to the new list, the new item should be displayed
        using var homeResponseList2 = await client.GetAsync($"/?list={newList.UserListId}");
        homeResponseList2.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await homeResponseList2.Content.ReadAsStringAsync();
        Assert.Contains("New test list (1)", homePage);
        Assert.Contains("test item 1 on new test list", homePage);
    }

    [TestMethod]
    public async Task When_adding_item_on_new_list_Given_no_new_list_name_Should_add_to_all_list()
    {
        int listCount;
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            listCount = await context.UserLists.CountAsync();
        }

        using var client = _factory.CreateAuthenticatedClient();
        using var homeResponse = await client.GetAsync($"/?list={_userList!.UserListId}");
        homeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var homePage = await homeResponse.Content.ReadAsStringAsync();
        Assert.Contains("All (0)", homePage);
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(homePage, "/items/add");

        using var postResponse = await client.PostAsync("/items/add", new FormUrlEncodedContent([
            KeyValuePair.Create("__RequestVerificationToken", validationToken),
            KeyValuePair.Create("list", "new"),
            KeyValuePair.Create("newlist", "   "),
            KeyValuePair.Create("description", "test item 1 not on a new test list")
        ]));
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await postResponse.Content.ReadAsStringAsync();
        Assert.Contains("All (1)", homePage);
        Assert.DoesNotContain("test item 1 not on a new test list", homePage);

        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            Assert.AreEqual(listCount, await context.UserLists.CountAsync());
        }

        // now switching to the all list, the new item should be displayed
        using var homeResponseAllList = await client.GetAsync("/?list=all");
        homeResponseAllList.StatusCode.Should().Be(HttpStatusCode.OK);
        homePage = await homeResponseAllList.Content.ReadAsStringAsync();
        Assert.Contains("All (1)", homePage);
        Assert.Contains("test item 1 not on a new test list", homePage);
    }

    [TestCleanup]
    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}