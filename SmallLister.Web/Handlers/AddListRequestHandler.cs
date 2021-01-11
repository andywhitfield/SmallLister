using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class AddListRequestHandler : IRequestHandler<AddListRequest>
    {
        private readonly ILogger<AddListRequestHandler> _logger;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserListRepository _userListRepository;
        public AddListRequestHandler(ILogger<AddListRequestHandler> logger, IUserAccountRepository userAccountRepository, IUserListRepository userListRepository)
        {
            _logger = logger;
            _userAccountRepository = userAccountRepository;
            _userListRepository = userListRepository;
        }

        public async Task<Unit> Handle(AddListRequest request, CancellationToken cancellationToken)
        {
            var user = await _userAccountRepository.GetUserAccountAsync(request.User);
            _logger.LogInformation($"Adding new list: {request.Model.Name}");
            await _userListRepository.AddListAsync(user, request.Model.Name?.Trim());
            return Unit.Value;
        }
    }
}