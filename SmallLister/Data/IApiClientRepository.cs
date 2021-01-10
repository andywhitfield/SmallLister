using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IApiClientRepository
    {
         Task<List<ApiClient>> GetAsync(UserAccount createdBy);
         Task<ApiClient> GetAsync(string appKey);
         Task CreateAsync(string displayName, string redirectUri, string appKey, string appSecretHash, string appSecretSalt, UserAccount createdBy);
    }
}