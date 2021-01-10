using System.Collections.Generic;

namespace SmallLister.Web.Model.Response
{
    public class GetListResponse
    {
        public GetListResponse(string name, IEnumerable<ItemResponse> items)
        {
            Name = name;
            Items = items;
        }
        public string Name { get; }
        public IEnumerable<ItemResponse> Items { get; }
    }
}