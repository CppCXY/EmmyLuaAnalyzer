using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;

public class DidOpenTextDocumentParams
{
    /**
     * The document that was opened.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentItem TextDocument { get; set; }
}
