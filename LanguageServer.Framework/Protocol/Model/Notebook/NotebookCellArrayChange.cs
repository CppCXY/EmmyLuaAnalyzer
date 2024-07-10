using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

/**
 * A change describing how to move a `NotebookCell`
 * array from state S to S'.
 *
 * @since 3.17.0
 */
public class NotebookCellArrayChange
{
    /**
     * The start offset of the cell that changed.
     */
    [JsonPropertyName("start")]
    public uint Start { get; set; }

    /**
     * The number of deleted cells.
     */
    [JsonPropertyName("deleteCount")]
    public uint DeleteCount { get; set; }

    /**
     * The new cells, if any.
     */
    [JsonPropertyName("cells")]
    public List<NotebookCell>? Cells { get; set; }
}
