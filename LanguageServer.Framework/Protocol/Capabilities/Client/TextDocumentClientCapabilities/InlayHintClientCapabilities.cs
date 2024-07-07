using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class InlayHintClientCapabilities
{
    /**
     * Whether inlay hints support dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Indicates which properties a client can resolve lazily on an inlay
     * hint.
     */
    [JsonPropertyName("resolveSupport")]
    public InlayHintResolveSupportClientCapabilities? ResolveSupport { get; init; }
}

public class InlayHintResolveSupportClientCapabilities
{
    /**
     * The properties that a client can resolve lazily.
     */
    [JsonPropertyName("properties")]
    public List<string> Properties { get; init; } = null!;
}
