using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class ExecuteCommandClientCapabilities
{
    /**
     * Execute command capabilities.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }
}
