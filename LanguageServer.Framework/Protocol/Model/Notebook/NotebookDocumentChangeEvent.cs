using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

/**
 * A change event for a notebook document.
 *
 * @since 3.17.0
 */
public class NotebookDocumentChangeEvent
{
    /**
     * The changed meta data if any.
     */
    [JsonPropertyName("metadata")]
    public JsonDocument? Metadata { get; set; }

    /**
     * Changes to cells.
     */
    [JsonPropertyName("cells")]
    public NotebookDocumentChangeEventCell? Cells { get; set; }
}

public class NotebookDocumentChangeEventCell
{

    /**
     * Changes to the cell structure to add or
     * remove cells.
     */
    [JsonPropertyName("structure")]
    public NotebookDocumentChangeEventCellStruct? Structure { get; set; }

    /**
     * Changes to notebook cells properties like its
     * kind, execution summary or metadata.
     */
    [JsonPropertyName("data")]
    public List<NotebookCell> Data { get; set; } = null!;

    /**
     * Changes to the text content of notebook cells.
     */
    [JsonPropertyName("textContent")]
    public List<NotebookDocumentChangeEventCellTextContent> TextContent { get; set; } = null!;
}

public class NotebookDocumentChangeEventCellStruct
{
    /**
     * The kind of cell changes.
     */
    [JsonPropertyName("array")]
    public NotebookCellArrayChange Array { get; set; } = null!;

    /**
     * The range of the cell.
     */
    [JsonPropertyName("didOpen")]
    public List<TextDocumentItem>? DidOpen { get; set; }

    /**
     * Additional closed cell text documents.
     */
    [JsonPropertyName("didClose")]
    public List<TextDocumentIdentifier>? DidClose { get; set; }
}

public class NotebookDocumentChangeEventCellTextContent
{
    [JsonPropertyName("document")]
    public VersionedTextDocumentIdentifier Document { get; set; } = null!;

    [JsonPropertyName("changes")]
    public List<TextDocumentContentChangeEvent> Changes { get; set; } = null!;
}
