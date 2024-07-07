using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

[JsonConverter(typeof(StringOrIntConverter))]
public readonly record struct StringOrInt
{
    public int IntValue { get; } = 0;

    public string? StringValue { get; }

    public StringOrInt(string value)
    {
        StringValue = value;
    }

    public StringOrInt(int value)
    {
        IntValue = value;
    }

    public static implicit operator StringOrInt(string item1) => new(item1);

    public static implicit operator StringOrInt(int item2) => new(item2);
}

public class StringOrIntConverter : JsonConverter<StringOrInt>
{
    public override StringOrInt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return new StringOrInt(reader.GetInt32());
        }

        return new StringOrInt(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, StringOrInt value, JsonSerializerOptions options)
    {
        if (value.StringValue != null)
        {
            writer.WriteStringValue(value.StringValue);
        }
        else
        {
            writer.WriteNumberValue(value.IntValue);
        }
    }
}
