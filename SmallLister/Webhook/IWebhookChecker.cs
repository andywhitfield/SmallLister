using System.Threading;
using System.Threading.Tasks;

namespace SmallLister.Webhook;

public interface IWebhookChecker
{
    Task CheckAsync(CancellationToken cancellationToken);
}