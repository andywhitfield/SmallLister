using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmallLister.Data;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers
{
    public class SignedInHandler : IRequestHandler<SignedInRequest, string>
    {
        private readonly IUserAccountRepository _userAccountRepository;

        public SignedInHandler(IUserAccountRepository userAccountRepository) => _userAccountRepository = userAccountRepository;

        public async Task<string> Handle(SignedInRequest request, CancellationToken cancellationToken)
        {
            if (await _userAccountRepository.GetUserAccountOrNullAsync(request.User) == null)
                await _userAccountRepository.CreateNewUserAsync(request.User);
            
            if (!string.IsNullOrEmpty(request.ReturnUrl) && Uri.TryCreate(request.ReturnUrl, UriKind.Relative, out var redirectUri))
                return redirectUri.ToString();

            return "~/";
        }
    }
}