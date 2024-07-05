using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

[JsonConverter(typeof(TraceValueConverter))]
public readonly record struct TraceValue(string Value)
{
    public static readonly TraceValue Off = new("off");
    
    public static readonly TraceValue Messages = new("messages");
    
    public static readonly TraceValue Verbose = new("verbose");

    public string Value { get; } = Value;
}

public class TraceValueConverter : JsonConverter<TraceValue>
{
    public override TraceValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new TraceValue(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, TraceValue value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}