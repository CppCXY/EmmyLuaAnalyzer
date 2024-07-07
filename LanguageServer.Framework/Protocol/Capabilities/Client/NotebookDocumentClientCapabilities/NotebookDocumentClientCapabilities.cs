using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.NotebookDocumentClientCapabilities;

public class NotebookDocumentClientCapabilities
{
    /**
     * Capabilities specific to notebook document synchronization
     *
     * @since 3.17.0
     */
    [JsonPropertyName("synchronization")]
    public NotebookDocumentSyncClientCapabilities? Synchronization { get; init; }
}
