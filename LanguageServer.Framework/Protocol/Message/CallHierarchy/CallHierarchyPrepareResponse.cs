using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

[JsonConverter(typeof(CallHierarchyPrepareResponseJsonConverter))]
public class CallHierarchyPrepareResponse(List<CallHierarchyItem>? result)
{
    public List<CallHierarchyItem>? Result { get; set; } = result;
}

public class CallHierarchyPrepareResponseJsonConverter : JsonConverter<CallHierarchyPrepareResponse>
{
    public override CallHierarchyPrepareResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new UnreachableException();
    }

    public override void Write(Utf8JsonWriter writer, CallHierarchyPrepareResponse value, JsonSerializerOptions options)
    {
        if (value.Result != null)
        {
            JsonSerializer.Serialize(writer, value.Result, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
