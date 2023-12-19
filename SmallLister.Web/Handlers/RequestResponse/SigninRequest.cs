using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse;

public class SigninRequest(string email) : IRequest<SigninResponse>
{
    public string Email { get; } = email;
}