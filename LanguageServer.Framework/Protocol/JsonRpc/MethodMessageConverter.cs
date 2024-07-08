using System.Text.Json;
using System.Text.Json.Serialization;

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
             if (root.TryGetProperty("id", out var id))
             {
                 if (id.ValueKind == JsonValueKind.Number)
                 {
                     return new RequestMessage(id.GetInt32(), method, JsonDocument.Parse(paramsJson));
                 }
                 else if (id.ValueKind == JsonValueKind.String)
                 {
                     return new RequestMessage(id.GetString()!, method, JsonDocument.Parse(paramsJson));
                 }
             }
             else
             {
                 return new NotificationMessage(method, JsonDocument.Parse(paramsJson));
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

