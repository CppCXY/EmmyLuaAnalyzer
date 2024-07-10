using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.NotebookDocument;

/**
 * The params sent in a close notebook document notification.
 *
 * @since 3.17.0
 */
public class DidCloseNotebookDocumentParams
{
    /**
     * The notebook document that got closed.
     */
    [JsonPropertyName("notebookDocument")]
    public NotebookDocumentIdentifier NotebookDocument { get; set; } = null!;

    /**
     * The text documents that represent the content
     * of a notebook cell that got closed.
     */
    [JsonPropertyName("cellTextDocuments")]
    public List<TextDocumentIdentifier> CellTextDocuments { get; set; } = null!;
}
