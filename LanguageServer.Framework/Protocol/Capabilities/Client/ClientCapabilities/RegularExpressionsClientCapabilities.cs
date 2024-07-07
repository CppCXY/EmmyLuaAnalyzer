using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class RegularExpressionsClientCapabilities
{
    /**
     * The client has support for regular expressions in the find widget.
     */
    [JsonPropertyName("engine")]
    public string? Engine { get; init; }

    /**
     * The engine's version.
     */
    [JsonPropertyName("version")]
    public string? Version { get; init; }
}
