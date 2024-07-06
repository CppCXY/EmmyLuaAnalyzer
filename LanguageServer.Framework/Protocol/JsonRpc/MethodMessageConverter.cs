using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

namespace EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

public class MethodMessageConverter : JsonConverter<MethodMessage>
{
    public override MethodMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        var method = root.GetProperty("method").GetString();
        var paramsJson = root.GetProperty("params").GetRawText();
        if (method is not null)
        {
            var methodType = JsonRpcHelper.GetMethodType(method);
            var methodParams = JsonSerializer.Deserialize(paramsJson, methodType, options);
            if (root.TryGetProperty("id", out var id))
            {
                if (id.ValueKind == JsonValueKind.Number)
                {
                    return new RequestMessage(id.GetInt32(), method, methodParams);
                }
                else if (id.ValueKind == JsonValueKind.String)
                {
                    return new RequestMessage(id.GetString()!, method, methodParams);
                }
            }
            else
            {
                return new NotificationMessage(method, methodParams);
            }
        }

        // Fallback to default deserialization for other methods
        // This part needs to be adjusted based on your specific needs and structure
        return JsonSerializer.Deserialize<MethodMessage>(root.GetRawText(), options)!;
    }

    public override void Write(Utf8JsonWriter writer, MethodMessage value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

