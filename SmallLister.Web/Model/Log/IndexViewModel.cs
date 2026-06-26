using SmallLister.Web.Handlers.RequestResponse;

namespace SmallLister.Web.Model.Log;

public class IndexViewModel(HttpContext context, IAsyncEnumerable<ActionLogResponse> actions)
    : BaseViewModel(context)
{
    public IAsyncEnumerable<ActionLogResponse> Actions => actions;
}
