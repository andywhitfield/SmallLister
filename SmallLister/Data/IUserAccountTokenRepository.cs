using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserAccountTokenRepository
    {
        Task<List<UserAccountToken>> GetAsync(UserAccountApiAccess userAccountApiAccess);
        Task<UserAccountToken> GetAsync(string tokenData);
        Task CreateAsync(UserAccountApiAccess userAccountApiAccess, string tokenData, DateTime expires);
        Task<UserAccountToken> GetLatestAsync(UserAccountApiAccess userAccountApiAccess);
    }
}