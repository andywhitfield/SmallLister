namespace SmallLister.Web.Model.Profile
{
    public class FeedModel
    {
        public FeedModel(int userFeedId, string userFeedIdentifier, string createdDate, string summaryInfo)
        {
            UserFeedId = userFeedId;
            UserFeedIdentifier = userFeedIdentifier;
            CreatedDate = createdDate;
            SummaryInfo = summaryInfo;

        }
        public int UserFeedId { get; }
        public string UserFeedIdentifier { get; }
        public string CreatedDate { get; }
        public string SummaryInfo { get; }
    }
}