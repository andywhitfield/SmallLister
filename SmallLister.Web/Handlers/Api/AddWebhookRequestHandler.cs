using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse.Api;

namespace SmallLister.Web.Handlers.Api;

public class AddWebhookRequestHandler : IRequestHandler<AddWebhookRequest, bool>
{
    private readonly ILogger<AddWebhookRequestHandler> _logger;
    private readonly IUserWebhookRepository _userWebhookRepository;

    public AddWebhookRequestHandler(ILogger<AddWebhookRequestHandler> logger, IUserWebhookRepository userWebhookRepository)
    {
        _logger = logger;
        _userWebhookRepository = userWebhookRepository;
    }
    
    public async Task<bool> Handle(AddWebhookRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Registering web hook for user {request.User.UserAccountId}: {request.Model.WebhookType}={request.Model.Webhook}");
        if ((await _userWebhookRepository.GetWebhookAsync(request.User, request.Model.WebhookType)) != null)
        {
            _logger.LogWarning($"Webhook already exists for user {request.User.UserAccountId} and type {request.Model.WebhookType}, not adding another one");
            return false;
        }

        await _userWebhookRepository.AddWebhookAsync(request.User, request.Model.WebhookType, request.Model.Webhook);

        return true;
    }
}
