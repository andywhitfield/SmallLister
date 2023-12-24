using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class ApiAuthorizeRequestHandler(ILogger<ApiAuthorizeRequestHandler> logger, IApiClientRepository apiClientRepository,
    IUserAccountRepository userAccountRepository, IUserAccountApiAccessRepository userAccountApiAccessRepository)
    : IRequestHandler<ApiAuthorizeRequest, string?>
{
    public async Task<string?> Handle(ApiAuthorizeRequest request, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Model.RedirectUri, UriKind.Absolute, out var redirectUri))
            return null;

        if (!request.Model.AllowApiAuth)
            return GetRedirectUri("errorCode=user_declined");

        var apiClient = await apiClientRepository.GetAsync(request.Model.AppKey);
        if (apiClient == null)
        {
            logger.LogInformation($"Cannot find api client {request.Model.AppKey} with redirect uri {request.Model.RedirectUri}");
            return null;
        }

        if (apiClient.RedirectUri != request.Model.RedirectUri)
        {
            logger.LogInformation($"Api client {apiClient.AppKey} redirect uri {apiClient.RedirectUri} does not match request {request.Model.RedirectUri}");
            return null;
        }

        if (!apiClient.IsEnabled)
        {
            logger.LogInformation($"Api client {apiClient.AppKey} is not enabled");
            return null;
        }

        var refreshToken = GuidString.NewGuidString();
        await userAccountApiAccessRepository.Create(apiClient, await userAccountRepository.GetUserAccountAsync(request.User), refreshToken);

        return GetRedirectUri($"refreshToken={refreshToken}");

        string GetRedirectUri(string returnInfo) => $"{redirectUri}{(string.IsNullOrEmpty(redirectUri.Query) ? "?" : "&")}{returnInfo}";
    }
}