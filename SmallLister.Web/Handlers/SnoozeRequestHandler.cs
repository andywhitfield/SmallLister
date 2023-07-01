using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class SnoozeRequestHandler : IRequestHandler<SnoozeRequest, bool>
{
    private readonly ILogger<SnoozeRequestHandler> _logger;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly IUserListRepository _userListRepository;
    private readonly IUserActionsService _userActionsService;
    private readonly IWebhookQueueRepository _webhookQueueRepository;

    public SnoozeRequestHandler(ILogger<SnoozeRequestHandler> logger, IUserAccountRepository userAccountRepository,
        IUserItemRepository userItemRepository, IUserListRepository userListRepository,
        IUserActionsService userActionsService, IWebhookQueueRepository webhookQueueRepository)
    {
        _logger = logger;
        _userAccountRepository = userAccountRepository;
        _userItemRepository = userItemRepository;
        _userListRepository = userListRepository;
        _userActionsService = userActionsService;
        _webhookQueueRepository = webhookQueueRepository;
    }

    public async Task<bool> Handle(SnoozeRequest request, CancellationToken cancellationToken)
    {
        var user = await _userAccountRepository.GetUserAccountAsync(request.User);
        var item = await _userItemRepository.GetItemAsync(user, request.UserItemId);
        if (item == null)
        {
            _logger.LogInformation($"Could not find item {request.UserItemId}");
            return false;
        }

        if (item.NextDueDate == null)
        {
            _logger.LogInformation($"Cannot set a 'postponed until date' on an item without a due date. UserItemId={request.UserItemId}");
            return false;
        }

        var list = item.UserListId != null ? await _userListRepository.GetListAsync(user, item.UserListId.Value) : null;
        item.PostponedUntilDate = (item.PostponedUntilDate ?? item.NextDueDate.Value).AddDays(1);

        await _userItemRepository.SaveAsync(item, list, _userActionsService);
        await _webhookQueueRepository.OnListItemChangeAsync(user, item, WebhookEventType.Modify);
        return true;
    }
}