using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class UserAccountApiAccessRepository : IUserAccountApiAccessRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<UserAccountApiAccessRepository> _logger;

        public UserAccountApiAccessRepository(SqliteDataContext context, ILogger<UserAccountApiAccessRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<UserAccountApiAccess> GetAsync(int userAccountApiAccessId) =>
            _context.UserAccountApiAccesses.SingleOrDefaultAsync(a => a.UserAccountApiAccessId == userAccountApiAccessId && a.DeletedDateTime == null);

        public Task<List<UserAccountApiAccess>> GetAsync(UserAccount user) =>
            _context.UserAccountApiAccesses.Include(a => a.ApiClient).Where(a => a.UserAccount == user && a.DeletedDateTime == null && a.ApiClient.IsEnabled).ToListAsync();

        public Task<UserAccountApiAccess> GetByRefreshTokenAsync(string refreshToken) =>
            _context.UserAccountApiAccesses.SingleOrDefaultAsync(a => a.RefreshToken == refreshToken && a.DeletedDateTime == null && a.ApiClient.IsEnabled);

        public async Task Create(ApiClient apiClient, UserAccount userAccount, string refreshToken)
        {
            await _context.UserAccountApiAccesses.AddAsync(new UserAccountApiAccess
            {
                ApiClient = apiClient,
                UserAccount = userAccount,
                RefreshToken = refreshToken
            });
            await _context.SaveChangesAsync();
        }
    }
}