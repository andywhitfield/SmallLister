using System.Runtime.CompilerServices;
using MediatR;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class GetActionLogRequestHandler(
    IUserAccountRepository userAccountRepository,
    IUserListRepository userListRepository,
    IUserActionRepository userActionRepository,
    IUserActionsService userActionsService)
    : IStreamRequestHandler<GetActionLogRequest, GetActionLogResponse>
{
    public async IAsyncEnumerable<GetActionLogResponse> Handle(GetActionLogRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        await foreach (var userAction in userActionRepository.GetUndoRedoActionsAsync(user).WithCancellation(cancellationToken))
            yield return new GetActionLogResponse(userListRepository, userActionsService, user, userAction);
    }
}
