using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class AddListRequestHandler : IRequestHandler<AddListRequest>
{
    private readonly ILogger<AddListRequestHandler> _logger;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserListRepository _userListRepository;
    private readonly IWebhookQueueRepository _webhookQueueRepository;

    public AddListRequestHandler(ILogger<AddListRequestHandler> logger,
        IUserAccountRepository userAccountRepository,
        IUserListRepository userListRepository,
        IWebhookQueueRepository webhookQueueRepository)
    {
        _logger = logger;
        _userAccountRepository = userAccountRepository;
        _userListRepository = userListRepository;
        _webhookQueueRepository = webhookQueueRepository;
    }

    public async Task<Unit> Handle(AddListRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAccountRepository.GetUserAccountAsync(request.User);
        _logger.LogInformation($"Adding new list: {request.Model.Name}");
        var newList = await _userListRepository.AddListAsync(user, request.Model.Name?.Trim());
        await _webhookQueueRepository.OnListChangeAsync(user, newList, WebhookEventType.New);
        return Unit.Value;
    }
}