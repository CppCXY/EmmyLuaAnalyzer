using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;

public class DidCloseTextDocumentParams
{
    /**
     * The document that was closed.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentIdentifier TextDocument { get; set; } = null!;
}
