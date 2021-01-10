using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallLister.Web.Model.Profile
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context, IEnumerable<ExternalApiAccessModel> externalApiAccessList,
            IEnumerable<ExternalApiClientModel> externalApiClients) : base(context)
        {
            ExternalApiAccessList = externalApiAccessList;
            ExternalApiClients = externalApiClients;
        }
        public IEnumerable<ExternalApiAccessModel> ExternalApiAccessList { get; }
        public IEnumerable<ExternalApiClientModel> ExternalApiClients { get; }
    }
}