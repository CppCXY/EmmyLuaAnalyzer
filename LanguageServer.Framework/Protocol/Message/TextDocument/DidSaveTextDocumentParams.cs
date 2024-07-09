using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;

public class DidSaveTextDocumentParams
{
    /**
     * The document that was saved.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = null!;

    /**
     * Optional the content when saved. Depends on the includeText value
     * when the save notifcation was requested.
     */
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
