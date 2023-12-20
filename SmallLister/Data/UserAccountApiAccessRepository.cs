using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserAccountApiAccessRepository(SqliteDataContext context)
    : IUserAccountApiAccessRepository
{
    public Task<UserAccountApiAccess?> GetAsync(int userAccountApiAccessId) =>
        context.UserAccountApiAccesses.SingleOrDefaultAsync(a => a.UserAccountApiAccessId == userAccountApiAccessId && a.DeletedDateTime == null);

    public Task<List<UserAccountApiAccess>> GetAsync(UserAccount user) =>
        context.UserAccountApiAccesses.Include(a => a.ApiClient).Where(a => a.UserAccount == user && a.DeletedDateTime == null && a.ApiClient.IsEnabled).ToListAsync();

    public Task<UserAccountApiAccess?> GetByRefreshTokenAsync(string refreshToken) =>
        context.UserAccountApiAccesses.SingleOrDefaultAsync(a => a.RefreshToken == refreshToken && a.DeletedDateTime == null && a.ApiClient.IsEnabled);

    public async Task Create(ApiClient apiClient, UserAccount userAccount, string refreshToken)
    {
        await context.UserAccountApiAccesses.AddAsync(new UserAccountApiAccess
        {
            ApiClient = apiClient,
            UserAccount = userAccount,
            RefreshToken = refreshToken
        });
        await context.SaveChangesAsync();
    }

    public Task UpdateAsync(UserAccountApiAccess userAccountApiAccess)
    {
        userAccountApiAccess.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }
}