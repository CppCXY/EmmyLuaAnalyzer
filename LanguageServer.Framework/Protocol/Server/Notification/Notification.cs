using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

namespace EmmyLua.LanguageServer.Framework.Protocol.Server.Notification;

[JsonRpc("$/cancelRequest")]
public record CancelParams(OneOf2<int, string> Id)
{
    [JsonPropertyName("id"), JsonConverter(typeof(OneOf2JsonConverter<int, string>))]
    public OneOf2<int, string> Id { get; } = Id;
}

public class NotificationMessageConverter : JsonConverter<NotificationMessage>
{
    public override NotificationMessage? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        var method = root.GetProperty("method").GetString();
        var paramsJson = root.GetProperty("params").GetRawText();
        
        if (method == "$/cancelRequest")
        {
            var paramsObj = JsonSerializer.Deserialize<CancelParams>(paramsJson, options);
            return new NotificationMessage(method, paramsObj);
        }

        // Fallback to default deserialization for other methods
        // This part needs to be adjusted based on your specific needs and structure
        return JsonSerializer.Deserialize<NotificationMessage>(root.GetRawText(), options);
    }

    public override void Write(Utf8JsonWriter writer, NotificationMessage value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}