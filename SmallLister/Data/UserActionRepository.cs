using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Actions;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class UserActionRepository : IUserActionRepository
    {
        private readonly ILogger<UserActionRepository> _logger;
        private readonly SqliteDataContext _context;

        public UserActionRepository(ILogger<UserActionRepository> logger, SqliteDataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task CreateAsync(UserAccount user, string description, UserActionType actionType, string data)
        {
            var now = DateTime.UtcNow;
            var nextActionNumber = 1;
            await foreach (var currentAction in _context.UserActions.Where(a => a.UserAccount == user && a.DeletedDateTime == null && a.IsCurrent).OrderBy(a => a.ActionNumber).AsAsyncEnumerable())
            {
                currentAction.IsCurrent = false;
                currentAction.LastUpdateDateTime = now;
                nextActionNumber = currentAction.ActionNumber + 1;
            }

            await foreach (var redoAction in _context.UserActions.Where(a => a.UserAccount == user && a.DeletedDateTime == null && a.ActionNumber >= nextActionNumber).AsAsyncEnumerable())
            {
                redoAction.DeletedDateTime = now;
                redoAction.LastUpdateDateTime = now;
            }

            await _context.UserActions.AddAsync(new UserAction
            {
                UserAccount = user,
                Description = description,
                ActionType = actionType,
                UserActionData = data,
                IsCurrent = true,
                ActionNumber = nextActionNumber,
                CreatedDateTime = now
            });

            await _context.SaveChangesAsync();
        }

        public async Task<(UserAction UndoAction, UserAction RedoAction)> GetUndoRedoActionAsync(UserAccount user)
        {
            var currentAction = await _context.UserActions.FirstOrDefaultAsync(ua => ua.UserAccount == user && ua.DeletedDateTime == null && ua.IsCurrent);

            var redoActionQuery = _context.UserActions.Where(ua => ua.UserAccount == user && ua.DeletedDateTime == null);
            if (currentAction != null)
                redoActionQuery = redoActionQuery.Where(ua => ua.ActionNumber > currentAction.ActionNumber);

            var redoAction = await redoActionQuery.OrderBy(ua => ua.ActionNumber).FirstOrDefaultAsync();

            return (currentAction, redoAction);
        }

        public async Task SetActionUndoneAsync(UserAction undoAction)
        {
            var now = DateTime.UtcNow;
            undoAction.IsCurrent = false;
            undoAction.LastUpdateDateTime = now;

            var previousAction = await _context.UserActions
                .Where(ua => ua.UserAccountId == undoAction.UserAccountId && ua.ActionNumber < undoAction.ActionNumber && ua.DeletedDateTime == null)
                .OrderByDescending(ua => ua.ActionNumber)
                .FirstOrDefaultAsync();
            if (previousAction != null)
            {
                previousAction.IsCurrent = true;
                previousAction.LastUpdateDateTime = now;
            }

            _logger.LogInformation($"Marked item {undoAction.UserActionId} as undone; {previousAction?.UserActionId} is now the current action.");

            await _context.SaveChangesAsync();
        }

        public async Task SetActionRedoneAsync(UserAction redoAction)
        {
            var now = DateTime.UtcNow;
            await foreach (var previousCurrentItem in _context.UserActions.Where(ua => ua.UserAccountId == redoAction.UserAccountId && ua.IsCurrent && ua.DeletedDateTime == null).AsAsyncEnumerable())
            {
                previousCurrentItem.IsCurrent = false;
                previousCurrentItem.LastUpdateDateTime = now;
            }

            redoAction.IsCurrent = true;
            redoAction.LastUpdateDateTime = now;

            _logger.LogInformation($"Marked item {redoAction.UserActionId} as current action.");

            await _context.SaveChangesAsync();
        }
    }
}