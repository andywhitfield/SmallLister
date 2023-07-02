using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallLister.Webhook;

namespace SmallLister.Web;

public class WebhookBackgroundService : BackgroundService
{
    private readonly ILogger<WebhookBackgroundService> _logger;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _defaultPollPeriod = TimeSpan.FromMinutes(10);

    public WebhookBackgroundService(ILogger<WebhookBackgroundService> logger, IConfiguration config, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _config = config;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
                await WaitForPollPeriodOrWebhookNotificationAsync(
                    serviceScope.ServiceProvider.GetRequiredService<IWebhookNotification>(),
                    stoppingToken);

                stoppingToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Checking if there are any webhooks to send");
                _logger.LogInformation("No webhooks need sending, done.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogTrace("Service stopping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in webhooks background service");
            }
        }

        _logger.LogInformation("Webhook background service stopped.");
    }

    private async Task WaitForPollPeriodOrWebhookNotificationAsync(IWebhookNotification webhookNotification, CancellationToken stoppingToken)
    {
        var pollPeriod = _config.GetSection("Webhooks")?.GetValue("CheckPollPeriod", _defaultPollPeriod) ?? _defaultPollPeriod;
        _logger.LogDebug($"Waiting {pollPeriod} before checking for webhooks to send");

        var notificationToken = webhookNotification.NotificationToken;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, notificationToken);
            await Task.Delay(pollPeriod, cts.Token);
        }
        catch (OperationCanceledException) when (notificationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Notification token signalled");
        }
    }
}
