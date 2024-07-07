using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client;

public class NotebookDocumentSyncClientCapabilities
{
    /**
     * Whether implementation supports dynamic registration. If this is
     * set to `true`, the client supports the new
     * `(NotebookDocumentSyncRegistrationOptions & NotebookDocumentSyncOptions)`
     * return value for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports sending execution summary data per cell.
     */
    [JsonPropertyName("executionSummarySupport")]
    public bool? ExecutionSummarySupport { get; init; }
}
