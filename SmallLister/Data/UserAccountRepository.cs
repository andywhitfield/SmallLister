using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<UserAccountRepository> _logger;

        public UserAccountRepository(SqliteDataContext context, ILogger<UserAccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<UserAccount> GetAsync(int userAccountId) =>
            _context.UserAccounts.SingleOrDefaultAsync(a => a.UserAccountId == userAccountId && a.DeletedDateTime == null);

        public Task CreateNewUserAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            var newUser = new UserAccount { AuthenticationUri = authenticationUri };

            _context.UserAccounts.Add(newUser);
            return _context.SaveChangesAsync();
        }

        private string GetIdentifierFromPrincipal(ClaimsPrincipal user) => user?.FindFirstValue("sub");

        public Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user) => GetUserAccountOrNullAsync(user) ?? throw new ArgumentException($"No UserAccount for the user: {GetIdentifierFromPrincipal(user)}");

        public Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            if (string.IsNullOrWhiteSpace(authenticationUri))
                return null;

            return _context.UserAccounts.FirstOrDefaultAsync(ua => ua.AuthenticationUri == authenticationUri && ua.DeletedDateTime == null);
        }

        public Task SetLastSelectedUserListIdAsync(UserAccount user, int? lastSelectedUserListId)
        {
            user.LastSelectedUserListId = lastSelectedUserListId;
            user.LastUpdateDateTime = DateTime.UtcNow;
            return _context.SaveChangesAsync();
        }
    }
}