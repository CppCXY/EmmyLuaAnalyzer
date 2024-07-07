using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(CompletionItemKindJsonConverter))]
public readonly record struct CompletionItemKind(int EnumValue)
{
    public static readonly CompletionItemKind Text = new(1);
    public static readonly CompletionItemKind Method = new(2);
    public static readonly CompletionItemKind Function = new(3);
    public static readonly CompletionItemKind Constructor = new(4);
    public static readonly CompletionItemKind Field = new(5);
    public static readonly CompletionItemKind Variable = new(6);
    public static readonly CompletionItemKind Class = new(7);
    public static readonly CompletionItemKind Interface = new(8);
    public static readonly CompletionItemKind Module = new(9);
    public static readonly CompletionItemKind Property = new(10);
    public static readonly CompletionItemKind Unit = new(11);
    public static readonly CompletionItemKind Value = new(12);
    public static readonly CompletionItemKind Enum = new(13);
    public static readonly CompletionItemKind Keyword = new(14);
    public static readonly CompletionItemKind Snippet = new(15);
    public static readonly CompletionItemKind Color = new(16);
    public static readonly CompletionItemKind File = new(17);
    public static readonly CompletionItemKind Reference = new(18);
    public static readonly CompletionItemKind Folder = new(19);
    public static readonly CompletionItemKind EnumMember = new(20);
    public static readonly CompletionItemKind Constant = new(21);
    public static readonly CompletionItemKind Struct = new(22);
    public static readonly CompletionItemKind Event = new(23);
    public static readonly CompletionItemKind Operator = new(24);
    public static readonly CompletionItemKind TypeParameter = new(25);

    public int EnumValue { get; } = EnumValue;
}

public class CompletionItemKindJsonConverter : JsonConverter<CompletionItemKind>
{
    public override CompletionItemKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new CompletionItemKind(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, CompletionItemKind value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.EnumValue);
    }
}
