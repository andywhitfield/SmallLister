using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserAccountApiAccessRepository
    {
        Task<UserAccountApiAccess> GetAsync(int userAccountApiAccessId);
        Task<List<UserAccountApiAccess>> GetAsync(UserAccount user);
        Task<UserAccountApiAccess> GetByRefreshTokenAsync(string refreshToken);
        Task Create(ApiClient apiClient, UserAccount userAccount, string refreshToken);
    }
}