using System.Text.Json.Serialization;

namespace SmallLister.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookEventType
{
    New, Modify, Delete
}