using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class RangeFormattingClientCapabilities
{
    /**
     * Whether formatting supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Whether the client supports formatting multiple ranges at once.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("rangesSupport")]
    public bool? RangesSupport { get; init; }
}
