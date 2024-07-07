using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class WorkspaceSymbolClientCapabilities
{
    /**
     * Symbol request supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Specific capabilities for the `SymbolKind` in the `workspace/symbol` request.
     */
    [JsonPropertyName("symbolKind")]
    public SymbolKindClientCapabilities? SymbolKind { get; init; }

    /**
     * The client supports tags on `SymbolInformation` and `WorkspaceSymbol`.
     * Clients supporting tags have to handle unknown tags gracefully.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("tagSupport")]
    public SymbolTagSupportClientCapabilities? TagSupport { get; init; }

    /**
     * The client supports partial workspace symbols. The client will send the
     * request `workspaceSymbol/resolve` to the server to resolve additional
     * properties.
     *
     * @since 3.17.0 - proposedState
     */
    [JsonPropertyName("resolveSupport")]
    public WorkspaceSymbolResolveSupportClientCapabilities? ResolveSupport { get; init; }
}

public class WorkspaceSymbolResolveSupportClientCapabilities
{
    /**
     * The properties that a client can resolve lazily.
     */
    [JsonPropertyName("properties")]
    public List<string> Properties { get; init; } = null!;
}
