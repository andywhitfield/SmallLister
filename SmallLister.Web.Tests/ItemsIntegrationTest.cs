using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallLister.Data;
using SmallLister.Model;
using Xunit;

namespace SmallLister.Web.Tests;

public class ItemsIntegrationTest : IAsyncLifetime
{
    private readonly IntegrationTestWebApplicationFactory _factory = new IntegrationTestWebApplicationFactory();

    public async Task InitializeAsync()
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.Migrate();
        var userAccount = await context.UserAccounts.AddAsync(new UserAccount { Email = "test-user-1" });
        var userList = await context.UserLists.AddAsync(new UserList { Name = "Test list", UserAccount = userAccount.Entity });
        await context.UserItems.AddRangeAsync(
            new UserItem { Description = "Test item 1", UserAccount = userAccount.Entity },
            new UserItem { Description = "Test item 2", UserAccount = userAccount.Entity, UserList = userList.Entity, NextDueDate = DateTime.Today },
            new UserItem { Description = "Test item 3", UserAccount = userAccount.Entity, NextDueDate = DateTime.Today.AddDays(-1), Repeat = ItemRepeat.Daily }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Marking_daily_overdue_item_as_done_moves_it_to_due_today()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.Contain("1 overdue and 1 due today (2)")
            .And.Contain("All (3)")
            .And.Contain("All upcoming (2)")
            .And.Contain("Due yesterday", Exactly.Once())
            .And.Contain("Due today", Exactly.Once());

        var item = await GetUserItemWithDescriptionAsync("Test item 3");
        item?.NextDueDate.Should().Be(DateTime.Today.AddDays(-1));
        item?.LastUpdateDateTime.Should().BeNull();
        var doneAction = $"/items/done/{item?.UserItemId}";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(responseContent, doneAction);

        var beforeEdit = DateTime.UtcNow;
        response = await client.PostAsync(doneAction, new FormUrlEncodedContent(new[] { KeyValuePair.Create("__RequestVerificationToken", validationToken) }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.Contain("2 due today (2)")
            .And.Contain("All (3)")
            .And.Contain("All upcoming (2)")
            .And.Contain("Due today", Exactly.Twice());
        item = await GetUserItemWithDescriptionAsync("Test item 3");
        item?.NextDueDate.Should().Be(DateTime.Today);
        item?.LastUpdateDateTime.Should().BeAfter(beforeEdit);
    }

    [Fact]
    public async Task Marking_due_item_as_done()
    {
        using var client = _factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.Contain("1 overdue and 1 due today (2)")
            .And.Contain("All (3)")
            .And.Contain("All upcoming (2)")
            .And.Contain("Due yesterday", Exactly.Once())
            .And.Contain("Due today", Exactly.Once());

        var item = await GetUserItemWithDescriptionAsync("Test item 2");
        item?.NextDueDate.Should().Be(DateTime.Today);
        item?.LastUpdateDateTime.Should().BeNull();
        var doneAction = $"/items/done/{item?.UserItemId}";
        var validationToken = IntegrationTestWebApplicationFactory.GetFormValidationToken(responseContent, doneAction);

        var beforeEdit = DateTime.UtcNow;
        response = await client.PostAsync(doneAction, new FormUrlEncodedContent(new[] { KeyValuePair.Create("__RequestVerificationToken", validationToken) }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Logout")
            .And.Contain("1 overdue (1)")
            .And.Contain("All (2)")
            .And.Contain("All upcoming (1)")
            .And.Contain("Due yesterday", Exactly.Once());
        item = await GetUserItemWithDescriptionAsync("Test item 2");
        item?.NextDueDate.Should().Be(DateTime.Today);
        item?.CompletedDateTime.Should().BeAfter(beforeEdit);
        item?.LastUpdateDateTime.Should().BeAfter(beforeEdit);
    }

    private Task<UserItem?> GetUserItemWithDescriptionAsync(string description)
    {
        using var serviceScope = _factory.Services.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        return context.UserItems.FirstOrDefaultAsync(i => i.Description == description);
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}