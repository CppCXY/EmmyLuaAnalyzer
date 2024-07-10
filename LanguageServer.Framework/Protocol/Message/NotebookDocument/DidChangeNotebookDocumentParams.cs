using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.NotebookDocument;

/**
 * The params sent in a change notebook document notification.
 *
 * @since 3.17.0
 */
public class DidChangeNotebookDocumentParams
{
    /**
     * The notebook document that did change. The version number points
     * to the version after all provided changes have been applied.
     */
    [JsonPropertyName("notebookDocument")]
    public VersionedNotebookDocumentIdentifier NotebookDocument { get; set; } = null!;

    /**
     * The actual changes to the notebook document.
     *
     * The change describes a single state change to the notebook document,
     * so it moves a notebook document, its cells and its cell text document
     * contents from state S to S'.
     *
     * To mirror the content of a notebook using change events use the
     * following approach:
     * - start with the same initial content
     * - apply the 'notebookDocument/didChange' notifications in the order
     *   you receive them.
     */
    [JsonPropertyName("change")]
    public NotebookDocumentChangeEvent Change { get; set; } = null!;
}
