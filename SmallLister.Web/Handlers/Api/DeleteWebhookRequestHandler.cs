using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse.Api;

namespace SmallLister.Web.Handlers.Api;

public class DeleteWebhookRequestHandler : IRequestHandler<DeleteWebhookRequest>
{
    private readonly ILogger<DeleteWebhookRequestHandler> _logger;
    private readonly IUserWebhookRepository _userWebhookRepository;

    public DeleteWebhookRequestHandler(ILogger<DeleteWebhookRequestHandler> logger, IUserWebhookRepository userWebhookRepository)
    {
        _logger = logger;
        _userWebhookRepository = userWebhookRepository;
    }
    
    public async Task<Unit> Handle(DeleteWebhookRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Deleting web hook for user {request.User.UserAccountId} of type {request.WebhookType}");
        await _userWebhookRepository.DeleteWebhookAsync(request.User, request.WebhookType);
        return Unit.Value;
    }
}
