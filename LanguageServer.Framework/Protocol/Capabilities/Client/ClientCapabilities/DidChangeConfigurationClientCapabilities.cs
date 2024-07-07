using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class DidChangeConfigurationClientCapabilities
{
    /**
     * Whether change notifications are supported.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }
}
