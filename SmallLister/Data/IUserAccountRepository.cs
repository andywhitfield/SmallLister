using System.Security.Claims;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserAccountRepository
    {
        Task CreateNewUserAsync(ClaimsPrincipal user);
        Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user);
        Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user);
        Task SetLastSelectedUserListIdAsync(UserAccount user, int? lastSelectedUserListId);
    }
}