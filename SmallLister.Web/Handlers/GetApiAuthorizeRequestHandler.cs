using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class GetApiAuthorizeRequestHandler : IRequestHandler<GetApiAuthorizeRequest, GetApiAuthorizeResponse>
    {
        private readonly ILogger<GetApiAuthorizeRequestHandler> _logger;
        private readonly IApiClientRepository _apiClientRepository;

        public GetApiAuthorizeRequestHandler(ILogger<GetApiAuthorizeRequestHandler> logger, IApiClientRepository apiClientRepository)
        {
            _logger = logger;
            _apiClientRepository = apiClientRepository;
        }

        public async Task<GetApiAuthorizeResponse> Handle(GetApiAuthorizeRequest request, CancellationToken cancellationToken)
        {
            var apiClient = await _apiClientRepository.GetAsync(request.AppKey);
            if (apiClient == null)
            {
                _logger.LogInformation($"Cannot find api client {request.AppKey} with redirect uri {request.RedirectUri}");
                return GetApiAuthorizeResponse.InvalidResponse;
            }

            if (apiClient.RedirectUri != request.RedirectUri)
            {
                _logger.LogInformation($"Api client {apiClient.AppKey} redirect uri {apiClient.RedirectUri} does not match request {request.RedirectUri}");
                return GetApiAuthorizeResponse.InvalidResponse;
            }

            if (!apiClient.IsEnabled)
            {
                _logger.LogInformation($"Api client {apiClient.AppKey} is not enabled");
                return GetApiAuthorizeResponse.InvalidResponse;
            }

            return new GetApiAuthorizeResponse(apiClient.AppKey, apiClient.RedirectUri, apiClient.DisplayName);
        }
    }
}