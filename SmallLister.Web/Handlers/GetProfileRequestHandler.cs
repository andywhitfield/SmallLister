using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;
using SmallLister.Web.Model.Profile;

namespace SmallLister.Web.Handlers
{
    public class GetProfileRequestHandler : IRequestHandler<GetProfileRequest, GetProfileResponse>
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserAccountApiAccessRepository _userAccountApiAccessRepository;
        private readonly IUserAccountTokenRepository _userAccountTokenRepository;
        private readonly IApiClientRepository _apiClientRepository;

        public GetProfileRequestHandler(IUserAccountRepository userAccountRepository, IUserAccountApiAccessRepository userAccountApiAccessRepository,
            IUserAccountTokenRepository userAccountTokenRepository, IApiClientRepository apiClientRepository)
        {
            _userAccountRepository = userAccountRepository;
            _userAccountApiAccessRepository = userAccountApiAccessRepository;
            _userAccountTokenRepository = userAccountTokenRepository;
            _apiClientRepository = apiClientRepository;
        }

        public async Task<GetProfileResponse> Handle(GetProfileRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var apiAccesses = await _userAccountApiAccessRepository.GetAsync(user);
            var externalApiAccessModels = new List<ExternalApiAccessModel>();
            foreach (var apiAccess in apiAccesses)
            {
                var mostRecentToken = await _userAccountTokenRepository.GetLatestAsync(apiAccess);
                var lastAccessed = mostRecentToken?.CreatedDateTime ?? apiAccess.CreatedDateTime;
                externalApiAccessModels.Add(new ExternalApiAccessModel(apiAccess.UserAccountApiAccessId, apiAccess.ApiClient.DisplayName, apiAccess.CreatedDateTime, lastAccessed, apiAccess.RevokedDateTime));
            }

            var externalApiClients = await _apiClientRepository.GetAsync(user);
            return new GetProfileResponse(
                new List<FeedModel>(),
                externalApiAccessModels.OrderByDescending(a => a.RevokedDateTime ?? DateTime.MaxValue).ThenByDescending(a => a.LastAccessedDateTime),
                externalApiClients.OrderBy(a => a.DisplayName).Select(a => new ExternalApiClientModel(
                    a.ApiClientId, a.DisplayName, a.AppKey, a.RedirectUri, a.IsEnabled
                )));
        }
    }
}