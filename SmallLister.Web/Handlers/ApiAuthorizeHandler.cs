using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class ApiAuthorizeHandler : IRequestHandler<ApiAuthorizeRequest, string>
    {
        private readonly ILogger<ApiAuthorizeHandler> _logger;
        private readonly IApiClientRepository _apiClientRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserAccountApiAccessRepository _userAccountApiAccessRepository;

        public ApiAuthorizeHandler(ILogger<ApiAuthorizeHandler> logger, IApiClientRepository apiClientRepository,
            IUserAccountRepository userAccountRepository, IUserAccountApiAccessRepository userAccountApiAccessRepository)
        {
            _logger = logger;
            _apiClientRepository = apiClientRepository;
            _userAccountRepository = userAccountRepository;
            _userAccountApiAccessRepository = userAccountApiAccessRepository;
        }

        public async Task<string> Handle(ApiAuthorizeRequest request, CancellationToken cancellationToken)
        {
            if (!Uri.TryCreate(request.Model.RedirectUri, UriKind.Absolute, out var redirectUri))
                return null;

            if (!request.Model.AllowApiAuth)
                return GetRedirectUri("errorCode=user_declined");

            var apiClient = await _apiClientRepository.GetAsync(request.Model.AppKey);
            if (apiClient == null)
            {
                _logger.LogInformation($"Cannot find api client {request.Model.AppKey} with redirect uri {request.Model.RedirectUri}");
                return null;
            }

            if (apiClient.RedirectUri != request.Model.RedirectUri)
            {
                _logger.LogInformation($"Api client {apiClient.AppKey} redirect uri {apiClient.RedirectUri} does not match request {request.Model.RedirectUri}");
                return null;
            }

            if (!apiClient.IsEnabled)
            {
                _logger.LogInformation($"Api client {apiClient.AppKey} is not enabled");
                return null;
            }

            var refreshToken = GuidString.NewGuidString();
            await _userAccountApiAccessRepository.Create(apiClient, await _userAccountRepository.GetUserAccountAsync(request.User), refreshToken);

            return GetRedirectUri($"refreshToken={refreshToken}");

            string GetRedirectUri(string returnInfo) => $"{redirectUri}{(string.IsNullOrEmpty(redirectUri.Query) ? "?" : "&")}{returnInfo}";
        }
    }
}