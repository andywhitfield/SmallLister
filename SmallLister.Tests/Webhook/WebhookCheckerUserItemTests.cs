using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using SmallLister.Data;
using SmallLister.Webhook;

namespace SmallLister.Tests.Webhook;

[TestClass]
public sealed class WebhookCheckerUserItemTests
{
    #pragma warning disable CS8618

    private ServiceProvider _services;

    #pragma warning restore CS8618

    private readonly List<string> _sentWebhooks = new();

    [TestInitialize]
    public void Setup()
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

    [TestCleanup]
    public void Cleanup() => _services.Dispose();

    [TestMethod]
    public async Task New_user_item_should_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "New list", UserAccount = userAccount.Entity });
            var userItem = dbContext.UserItems.Add(new() { Description = "Item 1", UserList = userList.Entity, UserAccount = userAccount.Entity });
            dbContext.UserItemWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.New, UserItem = userItem.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserItem = userItem.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserItem = userItem.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListItemChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserItemWebhookQueue.ForEachAsync(x =>
            {
                Assert.IsNotNull(x.SentDateTime);
                Assert.IsTrue(x.SentDateTime.Value >= DateTime.UtcNow.AddSeconds(-1) && x.SentDateTime.Value <= DateTime.UtcNow);
                Assert.IsNotNull(x.SentPayload);
                Assert.AreEqual("""[{"ListId":"1","ListItemId":"1","Event":"New"}]""", x.SentPayload);
                Assert.AreEqual(1, _sentWebhooks.Count);
                Assert.AreEqual("""[{"ListId":"1","ListItemId":"1","Event":"New"}]""", _sentWebhooks[0]);
            });
        }
    }

    [TestMethod]
    public async Task Deleted_user_item_should_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "New list", UserAccount = userAccount.Entity });
            var userItem = dbContext.UserItems.Add(new() { Description = "Item 1", DeletedDateTime = DateTime.UtcNow, UserList = userList.Entity, UserAccount = userAccount.Entity });
            dbContext.UserItemWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.Modify, UserItem = userItem.Entity },
                new() { EventType = Model.WebhookEventType.Delete, UserItem = userItem.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListItemChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserItemWebhookQueue.ForEachAsync(x =>
            {
                Assert.IsNotNull(x.SentDateTime);
                Assert.IsTrue(x.SentDateTime.Value >= DateTime.UtcNow.AddSeconds(-1) && x.SentDateTime.Value <= DateTime.UtcNow);
                Assert.IsNotNull(x.SentPayload);
                Assert.AreEqual("""[{"ListId":"1","ListItemId":"1","Event":"Delete"}]""", x.SentPayload);
                Assert.AreEqual(1, _sentWebhooks.Count);
                Assert.AreEqual("""[{"ListId":"1","ListItemId":"1","Event":"Delete"}]""", _sentWebhooks[0]);
            });
        }
    }

    [TestMethod]
    public async Task Modified_user_item_should_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "New list", UserAccount = userAccount.Entity });
            var userItem = dbContext.UserItems.Add(new() { Description = "Item 1", UserList = userList.Entity, UserAccount = userAccount.Entity });
            dbContext.UserItemWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.Modify, UserItem = userItem.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserItem = userItem.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListItemChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserItemWebhookQueue.ForEachAsync(x =>
            {
                Assert.IsNotNull(x.SentDateTime);
                Assert.IsTrue(x.SentDateTime.Value >= DateTime.UtcNow.AddSeconds(-1) && x.SentDateTime.Value <= DateTime.UtcNow);
                Assert.IsNotNull(x.SentPayload);
                Assert.AreEqual("""[{"ListId":"1","ListItemId":"1","Event":"Modify"}]""", x.SentPayload);
                Assert.AreEqual(1, _sentWebhooks.Count);
                Assert.AreEqual("""[{"ListId":"1","ListItemId":"1","Event":"Modify"}]""", _sentWebhooks[0]);
            });
        }
    }

    [TestMethod]
    public async Task New_then_deleted_user_item_should_not_send_webhook()
    {
        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            var userAccount = dbContext.UserAccounts.Add(new() { Email = "user1/" });
            var userList = dbContext.UserLists.Add(new() { Name = "New list", UserAccount = userAccount.Entity });
            var userItem = dbContext.UserItems.Add(new() { Description = "Item 1", DeletedDateTime = DateTime.UtcNow, UserList = userList.Entity, UserAccount = userAccount.Entity });
            dbContext.UserItemWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.New, UserItem = userItem.Entity },
                new() { EventType = Model.WebhookEventType.Delete, UserItem = userItem.Entity });
            dbContext.UserWebhooks.Add(new() { UserAccount = userAccount.Entity, WebhookType = Model.WebhookType.ListItemChange, Webhook = new("http://uri/") });
            await dbContext.SaveChangesAsync();
        }

        var webhookChecker = _services.GetRequiredService<WebhookChecker>();
        await webhookChecker.CheckAsync(CancellationToken.None);

        await using (_services.CreateAsyncScope())
        {
            var dbContext = _services.GetRequiredService<SqliteDataContext>();
            await dbContext.UserItemWebhookQueue.ForEachAsync(x =>
            {
                // the sent date time should be set, but the payload will be null indicating nothing sent
                Assert.IsNotNull(x.SentDateTime);
                Assert.IsTrue(x.SentDateTime.Value >= DateTime.UtcNow.AddSeconds(-1) && x.SentDateTime.Value <= DateTime.UtcNow);
                Assert.IsNull(x.SentPayload);
                Assert.AreEqual(0, _sentWebhooks.Count);
            });
        }
    }
}
