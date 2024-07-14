using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.DocumentRender;

public class EmmyAnnotatorRequestParams
{
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; }
}

// TODO fix proto, 这里只是为了兼容老版本的emmylua的渲染
[JsonConverter(typeof(EmmyAnnotatorTypeJsonConverter))]
public enum EmmyAnnotatorType
{
    Param = 0,
    Global = 1,
    Upvalue = 3
}

public class EmmyAnnotatorTypeJsonConverter : JsonConverter<EmmyAnnotatorType>
{
    public override EmmyAnnotatorType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (EmmyAnnotatorType)reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, EmmyAnnotatorType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}

public class RenderRange(DocumentRange range)
{
    [JsonPropertyName("range")]
    public DocumentRange Range { get; set; } = range;
}

public class EmmyAnnotatorResponse(string uri, EmmyAnnotatorType type)
{
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; } = uri;
    
    [JsonPropertyName("ranges")]
    public List<RenderRange> Ranges { get; set; } = [];

    [JsonPropertyName("type")]
    public EmmyAnnotatorType Type { get; set; } = type;
}