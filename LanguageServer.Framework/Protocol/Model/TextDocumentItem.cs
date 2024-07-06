using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

[method: JsonConstructor]
public record struct TextDocumentItem(DocumentUri Uri, string LanguageId, int Version, string Text)
{
    /**
     * The text document's URI.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; } = Uri;

    /**
     * The text document's language identifier.
     */
    [JsonPropertyName("languageId")]
    public string LanguageId { get; set; } = LanguageId;

    /**
     * The version number of this document (it will increase after each change, including undo/redo).
     */
    [JsonPropertyName("version")]
    public int Version { get; set; } = Version;

    /**
     * The content of the opened text document.
     */
    [JsonPropertyName("text")]
    public string Text { get; set; } = Text;
}