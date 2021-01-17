using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Model;
using SmallLister.Security;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class CreateFeedRequestHandler : IRequestHandler<CreateFeedRequest>
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserFeedRepository _userFeedRepository;

        public CreateFeedRequestHandler(IUserAccountRepository userAccountRepository, IUserFeedRepository userFeedRepository)
        {
            _userAccountRepository = userAccountRepository;
            _userFeedRepository = userFeedRepository;
        }

        public async Task<Unit> Handle(CreateFeedRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            await _userFeedRepository.CreateAsync(user, GuidString.NewGuidString(), request.Model.Type ?? UserFeedType.Due, request.Model.Display ?? UserFeedItemDisplay.None);
            return Unit.Value;
        }
    }
}