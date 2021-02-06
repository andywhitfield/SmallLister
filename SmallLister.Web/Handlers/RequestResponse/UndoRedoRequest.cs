using System.Security.Claims;
using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class UndoRedoRequest : IRequest<bool>
    {
        public ClaimsPrincipal User { get; }
        public int ForwardOrBackCount { get; }
        public UndoRedoRequest(ClaimsPrincipal user, int forwardOrBackCount)
        {
            User = user;
            ForwardOrBackCount = forwardOrBackCount;
        }
    }
}