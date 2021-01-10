using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class CreateExternalClientHandler : IRequestHandler<CreateExternalClientRequest, CreateExternalClientResponse>
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IApiClientRepository _apiClientRepository;

        public CreateExternalClientHandler(IUserAccountRepository userAccountRepository, IApiClientRepository apiClientRepository)
        {
            _userAccountRepository = userAccountRepository;
            _apiClientRepository = apiClientRepository;
        }

        public async Task<CreateExternalClientResponse> Handle(CreateExternalClientRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var appKey = GuidString.NewGuidString();
            var appSecret = GuidString.NewGuidString();
            var (appSecretSalt, appSecretHash) = SaltHashHelper.CreateHash(appSecret);

            await _apiClientRepository.CreateAsync(request.Model.Name, request.Model.Uri, appKey, appSecretHash, appSecretSalt, user);
            return new CreateExternalClientResponse(request.Model.Name, request.Model.Uri, appKey, appSecret);
        }
    }
}