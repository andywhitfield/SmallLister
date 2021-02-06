using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class UndoRedoRequestHandler : IRequestHandler<UndoRedoRequest, bool>
    {
        private readonly ILogger<UndoRedoRequestHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserActionsService _userActionsService;
        public UndoRedoRequestHandler(ILogger<UndoRedoRequestHandler> logger, IUserAccountRepository userAccountRepository,
            IUserActionsService userActionsService)
        {
            _logger = logger;
            _userActionsService = userActionsService;
            _userAccountRepository = userAccountRepository;
        }

        public async Task<bool> Handle(UndoRedoRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            for (var i = 0; i < Math.Abs(request.ForwardOrBackCount); i++)
            {
                _logger.LogInformation($"Performing {(request.ForwardOrBackCount > 0 ? "redo" : "undo")} action {i + 1} of {Math.Abs(request.ForwardOrBackCount)} for user {user.UserAccountId}");
                var completedAction = request.ForwardOrBackCount > 0
                    ? await _userActionsService.RedoAsync(user)
                    : await _userActionsService.UndoAsync(user);

                if (!completedAction)
                {
                    _logger.LogWarning($"Could not complete undo/redo action: {user.UserAccountId}/{request.ForwardOrBackCount}");
                    return false;
                }
            }
            return true;
        }
    }
}