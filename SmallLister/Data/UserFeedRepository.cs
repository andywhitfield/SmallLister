using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmallLister.Model;

namespace SmallLister.Data;

public class UserFeedRepository(SqliteDataContext context, ILogger<UserFeedRepository> logger)
    : IUserFeedRepository
{
    public Task CreateAsync(UserAccount user, string uniqueFeedIdentifier, UserFeedType type, UserFeedItemDisplay display)
    {
        logger.LogInformation($"Creating new feed for user {user.UserAccountId}: {uniqueFeedIdentifier}; {type}; {display}");

        context.UserFeeds.Add(new UserFeed
        {
            UserAccount = user,
            UserFeedIdentifier = uniqueFeedIdentifier,
            FeedType = type,
            ItemDisplay = display
        });
        return context.SaveChangesAsync();
    }

    public Task<UserFeed?> GetAsync(int userFeedId) =>
        context.UserFeeds.SingleOrDefaultAsync(f => f.UserFeedId == userFeedId && f.DeletedDateTime == null);

    public Task<UserFeed?> FindByIdentifierAsync(string feedIdentifier) =>
        context.UserFeeds.SingleOrDefaultAsync(f => f.UserFeedIdentifier == feedIdentifier && f.DeletedDateTime == null);

    public Task<List<UserFeed>> GetAsync(UserAccount user) =>
        context.UserFeeds.Where(f => f.UserAccount == user && f.DeletedDateTime == null).OrderByDescending(f => f.CreatedDateTime).ToListAsync();

    public Task SaveAsync(UserFeed userFeed)
    {
        userFeed.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }
}