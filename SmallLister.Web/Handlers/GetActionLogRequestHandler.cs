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
    : IRequestHandler<GetActionLogRequest, GetActionLogResponse>
{
    public async Task<GetActionLogResponse> Handle(GetActionLogRequest request, CancellationToken cancellationToken)
    {
        var user = await userAccountRepository.GetUserAccountAsync(request.User);
        return new GetActionLogResponse(userListRepository, userActionsService, user, userActionRepository.GetUndoRedoActionsAsync(user));
    }
}
