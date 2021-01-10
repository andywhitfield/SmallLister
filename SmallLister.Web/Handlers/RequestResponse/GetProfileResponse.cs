using System.Collections.Generic;
using SmallLister.Web.Model.Profile;

namespace SmallLister.Web.Handlers.RequestResponse
{
    public class GetProfileResponse
    {
        public GetProfileResponse(IEnumerable<ExternalApiAccessModel> externalApiAccessList, IEnumerable<ExternalApiClientModel> externalApiClients)
        {
            ExternalApiAccessList = externalApiAccessList;
            ExternalApiClients = externalApiClients;
        }
        public IEnumerable<ExternalApiAccessModel> ExternalApiAccessList { get; }
        public IEnumerable<ExternalApiClientModel> ExternalApiClients { get; }
    }
}