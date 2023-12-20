namespace SmallLister.Webhook.Model;

public class ListChange
{
    public string ListId { get; set; } = "";
    public string Event { get; set; } = "";
}