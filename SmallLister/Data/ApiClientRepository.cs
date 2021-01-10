using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class ApiClientRepository : IApiClientRepository
    {
        private readonly SqliteDataContext _context;
        private readonly ILogger<ApiClientRepository> _logger;

        public ApiClientRepository(SqliteDataContext context, ILogger<ApiClientRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ApiClient>> GetAsync(UserAccount createdBy) =>
            await _context.ApiClients.Where(a => a.CreatedBy == createdBy && a.DeletedDateTime == null).ToListAsync();

        public async Task<ApiClient> GetAsync(string appKey) =>
            await _context.ApiClients.SingleOrDefaultAsync(a => a.AppKey == appKey && a.DeletedDateTime == null);

        public async Task CreateAsync(string displayName, string redirectUri, string appKey, string appSecretHash, string appSecretSalt,
            UserAccount createdBy)
        {
            await _context.ApiClients.AddAsync(new ApiClient
            {
                DisplayName = displayName,
                RedirectUri = redirectUri,
                AppKey = appKey,
                AppSecretHash = appSecretHash,
                AppSecretSalt = appSecretSalt,
                CreatedBy = createdBy
            });
            await _context.SaveChangesAsync();
        }
    }
}