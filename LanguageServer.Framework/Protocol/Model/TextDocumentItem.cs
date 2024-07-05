using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

public record struct TextDocumentItem
{
    /**
     * The text document's URI.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; }

    /**
     * The text document's language identifier.
     */
    [JsonPropertyName("languageId")]
    public string LanguageId { get; set; }

    /**
     * The version number of this document (it will increase after each change, including undo/redo).
     */
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /**
     * The content of the opened text document.
     */
    [JsonPropertyName("text")]
    public string Text { get; set; }
}