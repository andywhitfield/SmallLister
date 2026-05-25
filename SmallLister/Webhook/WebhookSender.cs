using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Webhook;

public class WebhookSender : IWebhookSender
{
    public const string WebhookSenderHttpClient = nameof(WebhookSender);

    private readonly ILogger<WebhookSender> _logger;
    private readonly IUserWebhookRepository _userWebhookRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAsyncPolicy _retryPolicy;

    public WebhookSender(
        ILogger<WebhookSender> logger,
        IUserWebhookRepository userWebhookRepository,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _userWebhookRepository = userWebhookRepository;
        _httpClientFactory = httpClientFactory;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(1, retryCount => TimeSpan.FromSeconds(retryCount), (ex, timeSpan, retryCount, _) =>
                _logger.LogWarning(ex, "Webhook request attempt #{RetryCount} failed, next attempt in {TotalMilliseconds}ms", retryCount, timeSpan.TotalMilliseconds)
            );
    }

    public async Task<IEnumerable<(T WebhookToSend, string PayloadSent)>> SendAsync<T>(IEnumerable<(UserAccount UserAccount, T WebhookToSend)> webhooksToSend, WebhookType webhookType, Func<T, Task>? beforeSendActionAsync = null)
    {
        List<(T WebhookToSend, string PayloadSent)> sent = new();
        foreach (var webhooksByUser in webhooksToSend.GroupBy(wh => wh.UserAccount))
        {
            _logger.LogDebug("Sending webhooks for user {UserAccountId}", webhooksByUser.Key.UserAccountId);
            var userWebhookForType = await _userWebhookRepository.GetWebhookAsync(webhooksByUser.Key, webhookType);
            if (userWebhookForType == null)
            {
                _logger.LogInformation("No webhooks for user {UserAccountId} and type {WebhookType}, nothing sent", webhooksByUser.Key.UserAccountId, webhookType);
                continue;
            }

            if (beforeSendActionAsync != null)
            {
                foreach (var wh in webhooksByUser)
                    await beforeSendActionAsync(wh.WebhookToSend);
            }

            var sentPayload = await SendAsync(userWebhookForType, webhooksByUser.Select(wh => wh.WebhookToSend));
            if (sentPayload != null)
                sent.AddRange(webhooksByUser.Select(wh => (wh.WebhookToSend, sentPayload)));
        }

        return sent;
    }

    private async Task<string?> SendAsync<T>(UserWebhook userWebhook, T webhookToSend)
    {
        var payload = JsonSerializer.Serialize(webhookToSend);
        _logger.LogInformation("Sending webhook [{Webhook}] for user {UserAccountId}: {Payload}", userWebhook.Webhook, userWebhook.UserAccountId, payload);

        var httpClient = _httpClientFactory.CreateClient(WebhookSenderHttpClient);
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                using var response = await httpClient.PostAsync(userWebhook.Webhook, new StringContent(payload, Encoding.UTF8, "application/json"));
                _logger.LogDebug("webhook response: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Successfully sent webhook: {Webhook}", userWebhook.Webhook);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send webhook: {Webhook}", userWebhook.Webhook);
            return null;
        }

        return payload;
    }
}
