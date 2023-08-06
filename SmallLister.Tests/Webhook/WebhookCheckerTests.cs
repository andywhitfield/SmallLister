using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Webhook;
using Xunit;

namespace SmallLister.Tests.Webhook;

public sealed class WebhookCheckerTests : IDisposable
{
    private readonly ServiceProvider _services;

    public WebhookCheckerTests()
    {
        _services = new ServiceCollection()
            .AddLogging(lb => lb.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace))
            .AddDbContext<SqliteDataContext>(o => o.UseSqlite("DataSource=file::memory:?cache=shared"))
            .AddScoped<IWebhookQueueRepository, WebhookQueueRepository>()
            .AddScoped<IWebhookNotification, WebhookNotification>()
            .AddScoped<IWebhookSender, WebhookSender>()
            .AddScoped<IUserWebhookRepository, UserWebhookRepository>()
            .AddHttpClient()
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
            var userList = dbContext.UserLists.Add(new() { Name = "New list", UserAccount = new() { AuthenticationUri = "user1/" } });
            dbContext.UserListWebhookQueue.AddRange(
                new() { EventType = Model.WebhookEventType.New, UserList = userList.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserList = userList.Entity },
                new() { EventType = Model.WebhookEventType.Modify, UserList = userList.Entity });
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
            });
        }
    }
}
