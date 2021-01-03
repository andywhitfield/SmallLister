using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;

namespace SmallLister.Web.Handlers
{
    public class SignedInHandler : IRequestHandler<SignedInRequest>
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public SignedInHandler(IUserAccountRepository userAccountRepository) => _userAccountRepository = userAccountRepository;

        public async Task<Unit> Handle(SignedInRequest request, CancellationToken cancellationToken)
        {
            if (await _userAccountRepository.GetUserAccountOrNullAsync(request.User) == null)
                await _userAccountRepository.CreateNewUserAsync(request.User);
            return Unit.Value;
        }
    }
}