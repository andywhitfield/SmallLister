using System.Collections.Generic;
using System.Threading.Tasks;
using SmallLister.Model;

namespace SmallLister.Data
{
    public interface IUserFeedRepository
    {
        Task<UserFeed> GetAsync(int userFeedId);
        Task<List<UserFeed>> GetAsync(UserAccount user);
        Task CreateAsync(UserAccount user, string uniqueFeedIdentifier, UserFeedType type, UserFeedItemDisplay display);
        Task SaveAsync(UserFeed userFeed);
    }
}