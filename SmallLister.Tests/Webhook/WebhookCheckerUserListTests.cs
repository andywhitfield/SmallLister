using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SmallLister.Data;
using SmallLister.Webhook;
using Xunit;

namespace SmallLister.Tests.Webhook;

public sealed class WebhookCheckerUserListTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly List<string> _sentWebhooks = new();

    public WebhookCheckerUserListTests()
    {
        Mock<HttpMessageHandler> mockMessageHandler = new();
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>(async (requestMessage, token) => _sentWebhooks.Add(await (requestMessage.Content?.ReadAsStringAsync(CancellationToken.None) ?? Task.FromResult(""))))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
        Mock<IHttpClientFactory> mockHttpClientFactory = new();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mockMessageHandler.Object));

        _services = new ServiceCollection()
            .AddLogging(lb => lb.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace))
            .AddDbContext<SqliteDataContext>(o => o.UseSqlite($"DataSource=file:mem{Guid.NewGuid():N}?mode=memory&cache=shared"))
            .AddScoped<IWebhookQueueRepository, WebhookQueueRepository>()
            .AddScoped<IWebhookNotification, WebhookNotification>()
            .AddScoped<IWebhookSender, WebhookSender>()
            .AddScoped<IUserWebhookRepository, UserWebhookRepository>()
            .AddSingleton(_ => mockHttpClientFactory.Object)
            .AddTransient<WebhookChecker>()
            .BuildServiceProvider();

        var dbContext = _services.GetRequiredService<SqliteDataContext>();
        dbContext.Migrate();
    }

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task New_user_list_should_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "New list", UserAccount = userAccount.Entity });
            dbContext.UserListWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.New, UserList = userList.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserList = userList.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserList = userList.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserListWebhookQueue.ForEachAsync(x =>
            {
                Assert.Null(x.SentDateTime);
                Assert.Null(x.SentPayload);
            });
        }

        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserListWebhookQueue.ForEachAsync(x =>
            {
                Assert.NotNull(x.SentDateTime);
                Assert.InRange(x.SentDateTime.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
                Assert.NotNull(x.SentPayload);
                Assert.Equal("""[{"ListId":"1","Event":"New"}]""", x.SentPayload);
                Assert.Single(_sentWebhooks);
                Assert.Equal("""[{"ListId":"1","Event":"New"}]""", _sentWebhooks[0]);
            });
        }
    }

    [Fact]
    public async Task Modified_user_list_should_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "list 1", UserAccount = userAccount.Entity });
            dbContext.UserListWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.Modify, UserList = userList.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserList = userList.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserListWebhookQueue.ForEachAsync(x =>
            {
                Assert.NotNull(x.SentDateTime);
                Assert.InRange(x.SentDateTime.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
                Assert.NotNull(x.SentPayload);
                Assert.Equal("""[{"ListId":"1","Event":"Modify"}]""", x.SentPayload);
                Assert.Single(_sentWebhooks);
                Assert.Equal("""[{"ListId":"1","Event":"Modify"}]""", _sentWebhooks[0]);
            });
        }
    }

    [Fact]
    public async Task Deleted_user_list_should_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "list 1", DeletedDateTime = DateTime.UtcNow, UserAccount = userAccount.Entity });
            dbContext.UserListWebhookQueue.Add(new() { EventType = Model.WebhookEventType.Delete, UserList = userList.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserListWebhookQueue.ForEachAsync(x =>
            {
                Assert.NotNull(x.SentDateTime);
                Assert.InRange(x.SentDateTime.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
                Assert.NotNull(x.SentPayload);
                Assert.Equal("""[{"ListId":"1","Event":"Delete"}]""", x.SentPayload);
                Assert.Single(_sentWebhooks);
                Assert.Equal("""[{"ListId":"1","Event":"Delete"}]""", _sentWebhooks[0]);
            });
        }
    }

    [Fact]
    public async Task New_then_deleted_user_list_should_not_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "list 1", DeletedDateTime = DateTime.UtcNow, UserAccount = userAccount.Entity });
            dbContext.UserListWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.New, UserList = userList.Entity },
                new() { EventType = Model.WebhookEventType.Delete, UserList = userList.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserListWebhookQueue.ForEachAsync(x =>
            {
                // the sent date time should be set, but the payload will be null indicating nothing sent
                Assert.NotNull(x.SentDateTime);
                Assert.InRange(x.SentDateTime.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
                Assert.Null(x.SentPayload);
                Assert.Empty(_sentWebhooks);
            });
        }
    }
}
