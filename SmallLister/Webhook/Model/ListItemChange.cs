namespace SmallLister.Webhook.Model;

public class ListItemChange
{
    public string ListId { get; set; } = "";
    public string ListItemId { get; set; } = "";
    public string Event { get; set; } = "";
}