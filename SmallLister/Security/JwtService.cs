using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Security;

public class JwtService(ILogger<JwtService> logger, IUserAccountRepository userAccountRepository,
    IUserAccountTokenRepository userAccountTokenRepository,
    IUserAccountApiAccessRepository userAccountApiAccessRepository)
    : IJwtService
{
    public async Task<UserAccount?> GetUserAccountAsync(ClaimsPrincipal user)
    {
        var userAccountToken = await userAccountTokenRepository.GetAsync(user.Identity?.Name ?? "");
        if (userAccountToken == null)
        {
            logger.LogInformation($"No user account token found for: {user.Identity?.Name}");
            return null;
        }

        var userAccountApiAccess = await userAccountApiAccessRepository.GetAsync(userAccountToken.UserAccountApiAccessId);
        if (userAccountApiAccess == null)
        {
            logger.LogInformation($"No user account api access found with id {userAccountToken.UserAccountApiAccessId}");
            return null;
        }

        if (userAccountApiAccess.RevokedDateTime != null)
        {
            logger.LogInformation($"User account api access {userAccountToken.UserAccountApiAccessId} has been revoked");
            return null;
        }

        return await userAccountRepository.GetAsync(userAccountApiAccess.UserAccountId);
    }
}