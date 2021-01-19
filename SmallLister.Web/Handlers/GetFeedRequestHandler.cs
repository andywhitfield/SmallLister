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

namespace SmallLister.Web.Handlers
{
    public class GetFeedRequestHandler : IRequestHandler<GetFeedRequest, string>
    {
        private readonly ILogger<GetFeedRequestHandler> _logger;
        private readonly IUserFeedRepository _userFeedRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUserItemRepository _userItemRepository;
        private readonly IFeedGenerator _feedGenerator;

        public GetFeedRequestHandler(ILogger<GetFeedRequestHandler> logger, IUserFeedRepository userFeedRepository,
            IUserAccountRepository userAccountRepository, IUserItemRepository userItemRepository, IFeedGenerator feedGenerator)
        {
            _logger = logger;
            _userFeedRepository = userFeedRepository;
            _userAccountRepository = userAccountRepository;
            _userItemRepository = userItemRepository;
            _feedGenerator = feedGenerator;
        }

        public async Task<string> Handle(GetFeedRequest request, CancellationToken cancellationToken)
        {
            var userFeed = await _userFeedRepository.FindByIdentifierAsync(request.FeedIdentifier);
            if (userFeed == null)
            {
                _logger.LogInformation($"No user feed found with identifier: {request.FeedIdentifier}");
                return null;
            }

            var user = await _userAccountRepository.GetAsync(userFeed.UserAccountId);
            if (user == null)
            {
                _logger.LogInformation($"No user account found, associated with feed identifier: {request.FeedIdentifier}");
                return null;
            }

            var (items, _, _) = await _userItemRepository.GetItemsAsync(user, null, new UserItemFilter
            {
                DueToday = userFeed.FeedType == UserFeedType.Due,
                Overdue = true
            });
            var itemHash = GenerateHash(items.Select(i => i.UserItemId).Append(DateTime.Today.GetHashCode()));
            if (userFeed.ItemHash != itemHash)
            {
                userFeed.ItemHash = itemHash;
                await _userFeedRepository.SaveAsync(userFeed);
            }

            return _feedGenerator.GenerateFeed(request.BaseUri, userFeed.LastUpdateDateTime ?? userFeed.CreatedDateTime, items, userFeed).ToXmlString();
        }

        private int GenerateHash(IEnumerable<int> ids)
        {
            var hash = 23;
            foreach (var id in ids)
                hash = hash * 31 + id;
            return hash;
        }
    }
}