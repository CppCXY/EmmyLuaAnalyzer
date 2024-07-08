using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Union;

[JsonConverter(typeof(TextDocumentSyncOptionsOrKindConverter))]
public class TextDocumentSyncOptionsOrKind
{
    public TextDocumentSyncOptions? Value { get; } = null;

    public TextDocumentSyncKind? KindValue { get; } = default;

    public TextDocumentSyncOptionsOrKind(TextDocumentSyncOptions value)
    {
        Value = value;
    }

    public TextDocumentSyncOptionsOrKind(TextDocumentSyncKind value)
    {
        KindValue = value;
    }

    public static implicit operator TextDocumentSyncOptionsOrKind(TextDocumentSyncOptions value) => new(value);

    public static implicit operator TextDocumentSyncOptionsOrKind(TextDocumentSyncKind value) => new(value);
}

public class TextDocumentSyncOptionsOrKindConverter : JsonConverter<TextDocumentSyncOptionsOrKind>
{
    public override TextDocumentSyncOptionsOrKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return new TextDocumentSyncOptionsOrKind(JsonSerializer.Deserialize<TextDocumentSyncOptions>(ref reader, options)!);
        }
        else
        {
            return new TextDocumentSyncOptionsOrKind(JsonSerializer.Deserialize<TextDocumentSyncKind>(ref reader, options)!);
        }
    }

    public override void Write(Utf8JsonWriter writer, TextDocumentSyncOptionsOrKind value, JsonSerializerOptions options)
    {
        if (value.Value != null)
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.KindValue, options);
        }
    }
}
