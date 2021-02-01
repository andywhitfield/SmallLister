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
    }
}