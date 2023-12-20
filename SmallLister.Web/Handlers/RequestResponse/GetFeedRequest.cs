using MediatR;

namespace SmallLister.Web.Handlers.RequestResponse;

public class GetFeedRequest(string baseUri, string feedIdentifier) : IRequest<string?>
{
    public string BaseUri { get; } = baseUri;
    public string FeedIdentifier { get; } = feedIdentifier;
}