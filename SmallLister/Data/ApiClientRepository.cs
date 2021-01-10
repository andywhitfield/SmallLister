using System;
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

        public Task<List<ApiClient>> GetAsync(UserAccount createdBy) =>
            _context.ApiClients.Where(a => a.CreatedBy == createdBy && a.DeletedDateTime == null).ToListAsync();

        public Task<ApiClient> GetAsync(int apiClientId) =>
            _context.ApiClients.SingleOrDefaultAsync(a => a.ApiClientId == apiClientId && a.DeletedDateTime == null);

        public Task<ApiClient> GetAsync(string appKey) =>
            _context.ApiClients.SingleOrDefaultAsync(a => a.AppKey == appKey && a.DeletedDateTime == null);

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

        public Task UpdateAsync(ApiClient apiClient)
        {
            apiClient.LastUpdateDateTime = DateTime.UtcNow;
            return _context.SaveChangesAsync();
        }
    }
}