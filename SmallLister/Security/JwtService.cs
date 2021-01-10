using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Model;

namespace SmallLister.Security
{
    public class JwtService : IJwtService
    {
        private readonly ILogger<JwtService> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserAccountTokenRepository _userAccountTokenRepository;
        private readonly IUserAccountApiAccessRepository _userAccountApiAccessRepository;

        public JwtService(ILogger<JwtService> logger, IUserAccountRepository userAccountRepository,
            IUserAccountTokenRepository userAccountTokenRepository,
            IUserAccountApiAccessRepository userAccountApiAccessRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userAccountTokenRepository = userAccountTokenRepository;
            _userAccountApiAccessRepository = userAccountApiAccessRepository;
        }

        public async Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user)
        {
            var userAccountToken = await _userAccountTokenRepository.GetAsync(user.Identity.Name);
            if (userAccountToken == null)
            {
                _logger.LogInformation($"No user account token found for: {user.Identity.Name}");
                return null;
            }

            var userAccountApiAccess = await _userAccountApiAccessRepository.GetAsync(userAccountToken.UserAccountApiAccessId);
            if (userAccountApiAccess == null)
            {
                _logger.LogInformation($"No user account api access found with id {userAccountToken.UserAccountApiAccessId}");
                return null;
            }

            if (userAccountApiAccess.RevokedDateTime != null)
            {
                _logger.LogInformation($"User account api access {userAccountToken.UserAccountApiAccessId} has been revoked");
                return null;
            }

            return await _userAccountRepository.GetAsync(userAccountApiAccess.UserAccountId);
        }
    }
}