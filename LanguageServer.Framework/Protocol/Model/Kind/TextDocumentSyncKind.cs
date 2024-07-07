using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(TextDocumentSyncKindConverter))]
public readonly record struct TextDocumentSyncKind(int Value)
{
    public static TextDocumentSyncKind None { get; } = new(0);

    public static TextDocumentSyncKind Full { get; } = new(1);

    public static TextDocumentSyncKind Incremental { get; } = new(2);

    public int Value { get; } = Value;
}

public class TextDocumentSyncKindConverter : JsonConverter<TextDocumentSyncKind>
{
    public override TextDocumentSyncKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetInt32() switch
        {
            0 => TextDocumentSyncKind.None,
            1 => TextDocumentSyncKind.Full,
            2 => TextDocumentSyncKind.Incremental,
            _ => throw new JsonException()
        };
    }

    public override void Write(Utf8JsonWriter writer, TextDocumentSyncKind value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
