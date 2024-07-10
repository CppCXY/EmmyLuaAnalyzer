using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

/**
 * A versioned notebook document identifier.
 *
 * @since 3.17.0
 */
public class VersionedNotebookDocumentIdentifier
{
    /**
     * The version number of this notebook document.
     */
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /**
     * The notebook document's URI.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; }
}
