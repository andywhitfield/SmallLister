using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserAccountRepository(SqliteDataContext context, ILogger<UserAccountRepository> logger)
    : IUserAccountRepository
{
    public Task<UserAccount> GetAsync(int userAccountId) =>
        context.UserAccounts.SingleOrDefaultAsync(a => a.UserAccountId == userAccountId && a.DeletedDateTime == null);

    public async Task<UserAccount> CreateNewUserAsync(string email, byte[] credentialId, byte[] publicKey, byte[] userHandle)
    {
        var newUserAccount = context.UserAccounts.Add(new UserAccount { Email = email });
        context.UserAccountCredentials!.Add(new() { UserAccount = newUserAccount.Entity,
            CredentialId = credentialId, PublicKey = publicKey, UserHandle = userHandle });
        await context.SaveChangesAsync();
        return newUserAccount.Entity;
    }

    private string? GetEmailFromPrincipal(ClaimsPrincipal user)
    {
        logger.LogTrace($"Getting email from user: {user?.Identity?.Name}: [{string.Join(',', user?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Enumerable.Empty<string>())}]");
        return user?.FindFirstValue(ClaimTypes.Name);
    }

    public Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user) => GetUserAccountOrNullAsync(user) ?? throw new ArgumentException($"No UserAccount for the user: {GetEmailFromPrincipal(user)}");

    public Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user)
    {
        var email = GetEmailFromPrincipal(user);
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return context.UserAccounts.FirstOrDefaultAsync(ua => ua.Email == email && ua.DeletedDateTime == null);
    }

    public Task<UserAccount?> GetUserAccountByEmailAsync(string email)
        => context.UserAccounts!.FirstOrDefaultAsync(a => a.DeletedDateTime == null && a.Email == email);

    public Task SetLastSelectedUserListIdAsync(UserAccount user, int? lastSelectedUserListId)
    {
        user.LastSelectedUserListId = lastSelectedUserListId;
        user.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }

    public IAsyncEnumerable<UserAccountCredential> GetUserAccountCredentialsAsync(UserAccount user)
        => context.UserAccountCredentials!.Where(uac => uac.DeletedDateTime == null && uac.UserAccountId == user.UserAccountId).AsAsyncEnumerable();

    public Task<UserAccountCredential?> GetUserAccountCredentialsByUserHandleAsync(byte[] userHandle)
        => context.UserAccountCredentials!.FirstOrDefaultAsync(uac => uac.DeletedDateTime == null && uac.UserHandle.SequenceEqual(userHandle));

    public Task SetSignatureCountAsync(UserAccountCredential userAccountCredential, uint signatureCount)
    {
        userAccountCredential.SignatureCount = signatureCount;
        return context.SaveChangesAsync();
    }
}