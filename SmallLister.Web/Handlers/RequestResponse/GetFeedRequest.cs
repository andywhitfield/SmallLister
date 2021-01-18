using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetFeedRequest : IRequest<string>
    {
        public GetFeedRequest(string baseUri, string feedIdentifier)
        {
            BaseUri = baseUri;
            FeedIdentifier = feedIdentifier;
        }
        public string BaseUri { get; }
        public string FeedIdentifier { get; }
    }
}