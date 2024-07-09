using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;

/**
 * The parameters send in a will save text document notification.
 */
public class WillSaveTextDocumentParams
{
    /**
     * The document that will be saved.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = null!;

    /**
     * The 'TextDocumentSaveReason'.
     */
    [JsonPropertyName("reason")]
    public TextDocumentSaveReason Reason { get; set; }
}
