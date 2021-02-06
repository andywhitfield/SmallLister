using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Actions
{
    public class UpdateItemActionHandler : IUserActionHandler<UpdateItemAction>
    {
        private readonly ILogger<UpdateItemActionHandler> _logger;

        public UpdateItemActionHandler(ILogger<UpdateItemActionHandler> logger)
        {
            _logger = logger;
        }

        public UpdateItemAction GetUserAction(UserAction userAction) => UpdateItemAction.Create(userAction.UserActionData);

        public bool CanHandle(UserAction userAction, bool forUndo) => userAction.ActionType == UserActionType.UpdateItem;

        public async Task<bool> HandleAsync(UserAccount user, UserAction userAction, bool forUndo)
        {
            return true;
        }
    }
}