using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(SymbolKindJsonConverter))]
public readonly record struct SymbolKind(int Value)
{
    public static SymbolKind File = new SymbolKind(1);
    public static SymbolKind Module = new SymbolKind(2);
    public static SymbolKind Namespace = new SymbolKind(3);
    public static SymbolKind Package = new SymbolKind(4);
    public static SymbolKind Class = new SymbolKind(5);
    public static SymbolKind Method = new SymbolKind(6);
    public static SymbolKind Property = new SymbolKind(7);
    public static SymbolKind Field = new SymbolKind(8);
    public static SymbolKind Constructor = new SymbolKind(9);
    public static SymbolKind Enum = new SymbolKind(10);
    public static SymbolKind Interface = new SymbolKind(11);
    public static SymbolKind Function = new SymbolKind(12);
    public static SymbolKind Variable = new SymbolKind(13);
    public static SymbolKind Constant = new SymbolKind(14);
    public static SymbolKind String = new SymbolKind(15);
    public static SymbolKind Number = new SymbolKind(16);
    public static SymbolKind Boolean = new SymbolKind(17);
    public static SymbolKind Array = new SymbolKind(18);
    public static SymbolKind Object = new SymbolKind(19);
    public static SymbolKind Key = new SymbolKind(20);
    public static SymbolKind Null = new SymbolKind(21);
    public static SymbolKind EnumMember = new SymbolKind(22);
    public static SymbolKind Struct = new SymbolKind(23);
    public static SymbolKind Event = new SymbolKind(24);
    public static SymbolKind Operator = new SymbolKind(25);
    public static SymbolKind TypeParameter = new SymbolKind(26);

    public int Value { get; } = Value;
}

public class SymbolKindJsonConverter : JsonConverter<SymbolKind>
{
    public override SymbolKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new SymbolKind(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, SymbolKind value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
