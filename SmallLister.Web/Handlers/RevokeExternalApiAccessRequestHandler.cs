using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class RevokeExternalApiAccessRequestHandler : IRequestHandler<RevokeExternalApiAccessRequest, bool>
    {
        private readonly ILogger<RevokeExternalApiAccessRequestHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserAccountApiAccessRepository _userAccountApiAccessRepository;
        private readonly IApiClientRepository _apiClientRepository;

        public RevokeExternalApiAccessRequestHandler(ILogger<RevokeExternalApiAccessRequestHandler> logger, IUserAccountRepository userAccountRepository,
            IUserAccountApiAccessRepository userAccountApiAccessRepository, IApiClientRepository apiClientRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userAccountApiAccessRepository = userAccountApiAccessRepository;
            _apiClientRepository = apiClientRepository;
        }

        public async Task<bool> Handle(RevokeExternalApiAccessRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var userAccountApiAccess = await _userAccountApiAccessRepository.GetAsync(request.UserAccountApiAccessId);
            if (userAccountApiAccess == null)
            {
                _logger.LogInformation($"Could not find user account api access {request.UserAccountApiAccessId}");
                return false;
            }

            if (userAccountApiAccess.UserAccountId != user.UserAccountId)
            {
                _logger.LogInformation($"User account on api access {userAccountApiAccess.UserAccountId} does not match request user {user.UserAccountId}");
                return false;
            }

            userAccountApiAccess.RevokedDateTime = DateTime.UtcNow;
            await _userAccountApiAccessRepository.UpdateAsync(userAccountApiAccess);
            return true;
        }
    }
}