using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

/**
 * A notebook cell.
 *
 * A cell's document URI must be unique across ALL notebook
 * cells and can therefore be used to uniquely identify a
 * notebook cell or the cell's text document.
 *
 * @since 3.17.0
 */
public class NotebookCell
{
    /**
     * The cell's kind.
     */
    [JsonPropertyName("kind")]
    public NotebookCellKind Kind { get; set; }

    /**
     * The URI of the cell's text document
     * content.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri;

    /**
     * Additional metadata stored with the cell.
     */
    [JsonPropertyName("metadata")]
    public JsonDocument? Metadata { get; set; }

    /**
     * Additional execution summary information
     * if supported by the client.
     */
    [JsonPropertyName("executionSummary")]
    public ExecutionSummary? ExecutionSummary { get; set; }
}
