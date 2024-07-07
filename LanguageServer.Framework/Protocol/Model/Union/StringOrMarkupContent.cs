using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

[JsonConverter(typeof(StringOrMarkupContentConverter))]
public readonly record struct StringOrMarkupContent
{
    public string? StringValue { get; }

    public MarkupContent? MarkupContentValue { get; }

    public StringOrMarkupContent(string value)
    {
        StringValue = value;
    }

    public StringOrMarkupContent(MarkupContent value)
    {
        MarkupContentValue = value;
    }

    public static implicit operator StringOrMarkupContent(string item1) => new(item1);

    public static implicit operator StringOrMarkupContent(MarkupContent item2) => new(item2);
}

public class StringOrMarkupContentConverter : JsonConverter<StringOrMarkupContent>
{
    public override StringOrMarkupContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new StringOrMarkupContent(reader.GetString()!);
        }

        return new StringOrMarkupContent(JsonSerializer.Deserialize<MarkupContent>(ref reader, options)!);
    }

    public override void Write(Utf8JsonWriter writer, StringOrMarkupContent value, JsonSerializerOptions options)
    {
        if (value.StringValue != null)
        {
            writer.WriteStringValue(value.StringValue);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.MarkupContentValue, options);
        }
    }
}
