using System.Collections.Generic;

namespace SmallLister.Web.Model.Response
{
    public class GetAllListsResponse
    {
        public GetAllListsResponse(IEnumerable<ListResponse> lists) => Lists = lists;
        public IEnumerable<ListResponse> Lists { get; }

        public class ListResponse
        {
            public string ListId { get; }
            public string Name { get; }
            public ListResponse(string listId, string name)
            {
                ListId = listId;
                Name = name;
            }
        }
    }
}