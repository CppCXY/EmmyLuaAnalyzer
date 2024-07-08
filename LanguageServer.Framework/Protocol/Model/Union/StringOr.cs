using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

public class StringOr<T>
{
    public string? String { get; }

    public T? Value { get; }

    public StringOr(string? value)
    {
        String = value;
    }

    public StringOr(T value)
    {
        Value = value;
    }

    public static implicit operator StringOr<T>(string value) => new(value);

    public static implicit operator StringOr<T>(T value) => new(value);
}

public class StringOrJsonConverter<T> : JsonConverter<StringOr<T>>
{
    public override StringOr<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new StringOr<T>(reader.GetString());
        }

        return new StringOr<T>(JsonSerializer.Deserialize<T>(ref reader, options)!);
    }

    public override void Write(Utf8JsonWriter writer, StringOr<T> value, JsonSerializerOptions options)
    {
        if (value.String != null)
        {
            writer.WriteStringValue(value.String);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
