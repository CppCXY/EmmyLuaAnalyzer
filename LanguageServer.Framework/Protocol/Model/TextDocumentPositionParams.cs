using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

[method: JsonConstructor]
public record struct TextDocumentPositionParams(TextDocumentIdentifier TextDocument, Position Position)
{
    /**
     * The text document.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; } = TextDocument;

    /**
     * The position inside the text document.
     */
    [JsonPropertyName("position")] public Position Position { get; } = Position;
}