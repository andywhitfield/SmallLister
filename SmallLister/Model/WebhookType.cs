using System.Text.Json.Serialization;

namespace SmallLister.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookType
{
    ListChange, ListItemChange
}