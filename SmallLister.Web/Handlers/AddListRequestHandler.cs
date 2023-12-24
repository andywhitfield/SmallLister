using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class AddListRequestHandler(ILogger<AddListRequestHandler> logger,
    IUserAccountRepository userAccountRepository,
    IUserListRepository userListRepository,
    IWebhookQueueRepository webhookQueueRepository)
    : IRequestHandler<AddListRequest>
{
    public async Task<Unit> Handle(AddListRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        logger.LogInformation($"Adding new list: {request.Model.Name}");
        var newList = await userListRepository.AddListAsync(user, request.Model.Name?.Trim() ?? "");
        await webhookQueueRepository.OnListChangeAsync(user, newList, WebhookEventType.New);
        return Unit.Value;
    }
}