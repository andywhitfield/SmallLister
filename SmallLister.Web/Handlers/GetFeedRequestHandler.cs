using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmallLister.Data;
using SmallLister.Feed;
using SmallLister.Model;
using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Handlers;

public class GetFeedRequestHandler(ILogger<GetFeedRequestHandler> logger, IUserFeedRepository userFeedRepository,
    IUserAccountRepository userAccountRepository, IUserItemRepository userItemRepository, IFeedGenerator feedGenerator)
    : IRequestHandler<GetFeedRequest, string?>
{
    public async Task<string?> Handle(GetFeedRequest request, CancellationToken cancellationToken)
    {
        var userFeed = await userFeedRepository.FindByIdentifierAsync(request.FeedIdentifier);
        if (userFeed == null)
        {
            logger.LogInformation($"No user feed found with identifier: {request.FeedIdentifier}");
            return null;
        }

        var user = await userAccountRepository.GetAsync(userFeed.UserAccountId);
        if (user == null)
        {
            logger.LogInformation($"No user account found, associated with feed identifier: {request.FeedIdentifier}");
            return null;
        }

        var (items, _, _) = await userItemRepository.GetItemsAsync(user, null, new UserItemFilter
        {
            DueToday = userFeed.FeedType == UserFeedType.Due || userFeed.FeedType == UserFeedType.Daily,
            Overdue = true
        });
        var itemHash = GenerateHash(userFeed.FeedType == UserFeedType.Daily ? new[] { DateTime.Today.GetHashCode() } : items.Select(i => i.UserItemId).Append(DateTime.Today.GetHashCode()));
        if (userFeed.ItemHash != itemHash)
        {
            userFeed.ItemHash = itemHash;
            await userFeedRepository.SaveAsync(userFeed);
        }

        return feedGenerator.GenerateFeed(request.BaseUri, userFeed.LastUpdateDateTime ?? userFeed.CreatedDateTime, items, userFeed).ToXmlString();
    }

    private static int GenerateHash(IEnumerable<int> ids)
    {
        var hash = 23;
        foreach (var id in ids)
            hash = hash * 31 + id;
        return hash;
    }
}