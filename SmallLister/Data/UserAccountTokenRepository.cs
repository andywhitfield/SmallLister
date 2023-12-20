using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserAccountTokenRepository(SqliteDataContext context) : IUserAccountTokenRepository
{
    private readonly SqliteDataContext _context = context;

    public async Task<List<UserAccountToken>> GetAsync(UserAccountApiAccess userAccountApiAccess)
    {
        await DeleteExpiredTokensAsync();
        return await _context.UserAccountTokens.Where(t => t.UserAccountApiAccess == userAccountApiAccess && t.DeletedDateTime == null).ToListAsync();
    }

    public async Task<UserAccountToken?> GetAsync(string tokenData)
    {
        await DeleteExpiredTokensAsync();
        return await _context.UserAccountTokens.SingleOrDefaultAsync(t => t.TokenData == tokenData && t.DeletedDateTime == null);
    }

    public async Task CreateAsync(UserAccountApiAccess userAccountApiAccess, string tokenData, DateTime expires)
    {
        await DeleteExpiredTokensAsync();
        await _context.UserAccountTokens.AddAsync(new UserAccountToken
        {
            UserAccountApiAccess = userAccountApiAccess,
            ExpiryDateTime = expires,
            TokenData = tokenData
        });
        await _context.SaveChangesAsync();
    }

    public async Task<UserAccountToken?> GetLatestAsync(UserAccountApiAccess userAccountApiAccess)
    {
        await DeleteExpiredTokensAsync();
        return await _context.UserAccountTokens
            .Where(t => t.UserAccountApiAccess == userAccountApiAccess)
            .OrderByDescending(t => t.CreatedDateTime)
            .FirstOrDefaultAsync();
    }

    private async Task DeleteExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        await foreach (var token in _context.UserAccountTokens.Where(t => t.DeletedDateTime == null && t.ExpiryDateTime < now).AsAsyncEnumerable())
        {
            token.DeletedDateTime = now;
            token.LastUpdateDateTime = now;
        }
        await _context.SaveChangesAsync();
    }
}