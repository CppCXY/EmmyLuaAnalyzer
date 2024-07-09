using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

[JsonConverter(typeof(TextDocumentSaveReasonConverter))]
public readonly record struct TextDocumentSaveReason(int Value)
{
    /**
     * Manually triggered, e.g. by the user pressing save, by starting
     * debugging, or by an API call.
     */
    public static readonly TextDocumentSaveReason Manual = new TextDocumentSaveReason(1);
    /**
     * Automatic after a delay.
     */
    public static readonly TextDocumentSaveReason AfterDelay = new TextDocumentSaveReason(2);

    /**
     * When the editor lost focus.
     */
    public static readonly TextDocumentSaveReason FocusOut = new TextDocumentSaveReason(3);

    public int Value { get; } = Value;
}

public class TextDocumentSaveReasonConverter : JsonConverter<TextDocumentSaveReason>
{
    public override TextDocumentSaveReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new TextDocumentSaveReason(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, TextDocumentSaveReason value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
