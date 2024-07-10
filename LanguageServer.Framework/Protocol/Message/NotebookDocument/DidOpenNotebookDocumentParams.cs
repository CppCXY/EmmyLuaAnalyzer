using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.NotebookDocument;

/**
 * The params sent in an open notebook document notification.
 *
 * @since 3.17.0
 */
public class DidOpenNotebookDocumentParams
{
    /**
     * The notebook document that got opened.
     */
    [JsonPropertyName("notebookDocument")]
    public Model.Notebook.NotebookDocument NotebookDocument { get; set; } = null!;

    /**
     * The text documents that represent the content
     * of a notebook cell.
     */
    [JsonPropertyName("cellTextDocuments")]
    public List<TextDocumentItem> CellTextDocuments { get; set; } = null!;
}
