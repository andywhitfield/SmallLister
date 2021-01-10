using System.Security.Claims;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Security
{
    public interface IJwtService
    {
         Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user);
    }
}