using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class UpdateListRequestHandler : IRequestHandler<UpdateListRequest, bool>
{
    private readonly ILogger<UpdateListRequestHandler> _logger;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserListRepository _userListRepository;
    private readonly IWebhookQueueRepository _webhookQueueRepository;

    public UpdateListRequestHandler(ILogger<UpdateListRequestHandler> logger, IUserAccountRepository userAccountRepository,
        IUserListRepository userListRepository, IWebhookQueueRepository webhookQueueRepository)
    {
        _logger = logger;
        _userAccountRepository = userAccountRepository;
        _userListRepository = userListRepository;
        _webhookQueueRepository = webhookQueueRepository;
    }

    public async Task<bool> Handle(UpdateListRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAccountRepository.GetUserAccountAsync(request.User);
        var list = await _userListRepository.GetListAsync(user, request.UserListId);
        if (list == null)
        {
            _logger.LogInformation($"Could not find list {request.UserListId}");
            return false;
        }

        _logger.LogInformation($"Updating name of list {list.UserListId} [{list.Name}] to [{request.Model.Name}]");
        list.Name = request.Model.Name;
        await _userListRepository.SaveAsync(list);
        await _webhookQueueRepository.OnListChangeAsync(user, list, WebhookEventType.Modify);
        return true;
    }
}