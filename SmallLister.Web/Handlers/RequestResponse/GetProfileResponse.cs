using System.Collections.Generic;
using SmallLister.Web.Model.Profile;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetProfileResponse
    {
        public GetProfileResponse(IEnumerable<FeedModel> feeds, IEnumerable<ExternalApiAccessModel> externalApiAccessList, IEnumerable<ExternalApiClientModel> externalApiClients)
        {
            Feeds = feeds;
            ExternalApiAccessList = externalApiAccessList;
            ExternalApiClients = externalApiClients;
        }

        public IEnumerable<FeedModel> Feeds { get; }
        public IEnumerable<ExternalApiAccessModel> ExternalApiAccessList { get; }
        public IEnumerable<ExternalApiClientModel> ExternalApiClients { get; }
    }
}