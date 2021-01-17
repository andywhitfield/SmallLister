using System;
using SmallLister.Model;

namespace SmallLister.Web.Model.Profile
{
    public class FeedModel
    {
        public FeedModel(int userFeedId, string userFeedIdentifier, DateTime createdDate, UserFeedType feedType, UserFeedItemDisplay itemDisplay)
        {
            UserFeedId = userFeedId;
            UserFeedIdentifier = userFeedIdentifier;
            CreatedDate = createdDate.ToString("R");
            SummaryInfo =
                $"{(feedType switch { UserFeedType.Due => "Due and overdue items", UserFeedType.Overdue => "Overdue items only", _ => throw new InvalidOperationException("Unknown user feed type") })} " +
                $"{(itemDisplay switch { UserFeedItemDisplay.None => " with just a link to the item", UserFeedItemDisplay.ShortDescription => " with the item displaying the first word of the description only", UserFeedItemDisplay.Description => " with the item displaying the entire description", _ => throw new InvalidOperationException("Unknown user feed item display") })}";
        }
        public int UserFeedId { get; }
        public string UserFeedIdentifier { get; }
        public string CreatedDate { get; }
        public string SummaryInfo { get; }
    }
}