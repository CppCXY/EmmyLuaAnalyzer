using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

/**
 * A literal to identify a notebook document in the client.
 *
 * @since 3.17.0
 */
public class NotebookDocumentIdentifier
{
    /**
     * The notebook document's URI.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; set; }
}
