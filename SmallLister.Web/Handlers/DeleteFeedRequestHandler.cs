using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class DeleteFeedRequestHandler : IRequestHandler<DeleteFeedRequest, bool>
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserFeedRepository _userFeedRepository;

        public DeleteFeedRequestHandler(IUserAccountRepository userAccountRepository, IUserFeedRepository userFeedRepository)
        {
            _userAccountRepository = userAccountRepository;
            _userFeedRepository = userFeedRepository;
        }

        public async Task<bool> Handle(DeleteFeedRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            var userFeed = await _userFeedRepository.GetAsync(request.UserFeedId);
            if (userFeed == null || userFeed.UserAccountId != user.UserAccountId)
                return false;
            
            userFeed.DeletedDateTime = DateTime.UtcNow;
            await _userFeedRepository.SaveAsync(userFeed);
            
            return true;
        }
    }
}