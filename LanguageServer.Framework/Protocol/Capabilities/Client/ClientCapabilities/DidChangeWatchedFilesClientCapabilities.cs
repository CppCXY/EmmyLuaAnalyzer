using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class DidChangeWatchedFilesClientCapabilities
{
    /**
     * Whether the client supports dynamic registration for file changes.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }
}
