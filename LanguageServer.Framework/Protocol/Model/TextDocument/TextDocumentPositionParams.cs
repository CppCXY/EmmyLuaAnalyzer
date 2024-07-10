using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

public class TextDocumentPositionParams
{
    /**
     * The text document.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = null!;

    /**
     * The position inside the text document.
     */
    [JsonPropertyName("position")]
    public Position Position { get; set; }
}
