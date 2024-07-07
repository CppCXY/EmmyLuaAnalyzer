using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

/**
 * Enum of known token formats
 */
[JsonConverter(typeof(TokenFormatJsonConverter))]
public readonly record struct TokenFormat(string Value)
{
    public static readonly TokenFormat Relative = new("relative");

    public string Value { get; } = Value;
}

public class TokenFormatJsonConverter : JsonConverter<TokenFormat>
{
    public override TokenFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return new TokenFormat(value!);
    }

    public override void Write(Utf8JsonWriter writer, TokenFormat value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
