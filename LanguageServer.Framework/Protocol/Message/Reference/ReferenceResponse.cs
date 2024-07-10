using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;

[JsonConverter(typeof(ReferenceResponseJsonConverter))]
public class ReferenceResponse(List<Location> result)
{
    public List<Location>? Result { get; set; } = result;
}

public class ReferenceResponseJsonConverter : JsonConverter<ReferenceResponse>
{
    public override ReferenceResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new UnreachableException();
    }

    public override void Write(Utf8JsonWriter writer, ReferenceResponse value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Result, options);
    }
}
