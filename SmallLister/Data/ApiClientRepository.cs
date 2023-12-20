using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class ApiClientRepository(SqliteDataContext context) : IApiClientRepository
{
    public Task<List<ApiClient>> GetAsync(UserAccount createdBy) =>
        context.ApiClients.Where(a => a.CreatedBy == createdBy && a.DeletedDateTime == null).ToListAsync();

    public Task<ApiClient?> GetAsync(int apiClientId) =>
        context.ApiClients.SingleOrDefaultAsync(a => a.ApiClientId == apiClientId && a.DeletedDateTime == null);

    public Task<ApiClient?> GetAsync(string appKey) =>
        context.ApiClients.SingleOrDefaultAsync(a => a.AppKey == appKey && a.DeletedDateTime == null);

    public async Task CreateAsync(string displayName, string redirectUri, string appKey, string appSecretHash, string appSecretSalt,
        UserAccount createdBy)
    {
        await context.ApiClients.AddAsync(new ApiClient
        {
            DisplayName = displayName,
            RedirectUri = redirectUri,
            AppKey = appKey,
            AppSecretHash = appSecretHash,
            AppSecretSalt = appSecretSalt,
            CreatedBy = createdBy
        });
        await context.SaveChangesAsync();
    }

    public Task UpdateAsync(ApiClient apiClient)
    {
        apiClient.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }
}