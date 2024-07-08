using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

/**
 * Options specific to a notebook plus its cells
 * to be synced to the server.
 *
 * If a selector provides a notebook document
 * filter but no cell selector, all cells of a
 * matching notebook document will be synced.
 *
 * If a selector provides no notebook document
 * filter but only a cell selector, all notebook
 * documents that contain at least one matching
 * cell will be synced.
 *
 * @since 3.17.0
 */
public class NotebookDocumentSyncOptions
{
    /**
     * The notebooks to be synced
     */
    [JsonPropertyName("notebookSelector")]
    public List<NotebookSelectorOptions> NotebookSelector { get; init; } = null!;

    /**
     * Whether save notifications should be forwarded to
     * the server. Will only be honored if mode === `notebook`.
     */
    [JsonPropertyName("save")]
    public bool? Save { get; init; }
}

public class NotebookSelectorOptions
{
    /**
     * The notebook to be synced. If a string
     * value is provided, it matches against the
     * notebook type. '*' matches every notebook.
     */
    [JsonPropertyName("notebookType"), JsonConverter(typeof(StringOrJsonConverter<NotebookDocumentFilter>))]
    public StringOr<NotebookDocumentFilter>? NotebookType { get; init; }

    /**
     * The cells of the matching notebook to be synced.
     */
    [JsonPropertyName("cells")]
    public List<CellSelectorOptions>? Cells { get; init; }
}

/**
 * A notebook document filter denotes a notebook document by
 * different properties.
 *
 * @since 3.17.0
 */
public class NotebookDocumentFilter
{
    /**
     * The type of the enclosing notebook.
     */
    [JsonPropertyName("notebookType")]
    public string? NotebookType { get; init; }

    /**
     * A Uri [scheme](#Uri.scheme), like `file` or `untitled`.
    */
    [JsonPropertyName("scheme")]
    public string? Scheme { get; init; }

    /**
     * A glob pattern.
     */
    [JsonPropertyName("pattern")]
    public string? Pattern { get; init; }
}

/**
 * A cell selector denotes a cell in a notebook.
 *
 * @since 3.17.0
 */
public class CellSelectorOptions
{
    [JsonPropertyName("language")]
    public string Language { get; init; } = null!;
}
