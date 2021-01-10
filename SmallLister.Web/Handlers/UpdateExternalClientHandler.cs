using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class UpdateExternalClientHandler : IRequestHandler<UpdateExternalClientRequest, bool>
    {
        private readonly ILogger<UpdateExternalClientHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IApiClientRepository _apiClientRepository;

        public UpdateExternalClientHandler(ILogger<UpdateExternalClientHandler> logger, IUserAccountRepository userAccountRepository, IApiClientRepository apiClientRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _apiClientRepository = apiClientRepository;
        }

        public async Task<bool> Handle(UpdateExternalClientRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var apiClient = await _apiClientRepository.GetAsync(request.ApiClientId);
            if (apiClient == null)
            {
                _logger.LogInformation($"Could not find api client {request.ApiClientId}");
                return false;
            }

            if (apiClient.CreatedById != user.UserAccountId)
            {
                _logger.LogInformation($"Api client {apiClient.ApiClientId} is not owned by {user.UserAccountId} - cannot update");
                return false;
            }

            apiClient.IsEnabled = request.State == "enable";
            await _apiClientRepository.UpdateAsync(apiClient);
            return true;
        }
    }
}