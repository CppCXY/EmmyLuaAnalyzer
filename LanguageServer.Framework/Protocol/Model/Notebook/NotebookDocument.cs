using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

public class NotebookDocument
{
    /**
     * The notebook document's URI.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; }

    /**
     * The type of the notebook.
     */
    [JsonPropertyName("notebookType")]
    public string NotebookType { get; set; } = null!;

    /**
     * The version number of this document (it will increase after each
     * change, including undo/redo).
     */
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /**
     * The content of the notebook.
     */
    [JsonPropertyName("metadata")]
    public JsonDocument? Metadata { get; set; } = null!;

    /**
     * The cells of a notebook.
     */
    [JsonPropertyName("cells")]
    public List<NotebookCell> Cells { get; set; } = [];
}
