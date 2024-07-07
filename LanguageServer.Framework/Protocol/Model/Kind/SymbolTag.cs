using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(SymbolTagJsonConverter))]
public readonly record struct SymbolTag(int Value)
{
    public static readonly SymbolTag Deprecated = new(1);

    public int Value { get; } = Value;
}

public class SymbolTagJsonConverter : JsonConverter<SymbolTag>
{
    public override SymbolTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new SymbolTag(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, SymbolTag value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
