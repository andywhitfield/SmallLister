using System.Collections.Generic;
using System.Xml.Linq;
using SmallLister.Model;

namespace SmallLister.Feed
{
    public interface IFeedGenerator
    {
        XDocument GenerateFeed(string baseUri, IEnumerable<UserItem> items, UserFeed userFeed);
    }
}