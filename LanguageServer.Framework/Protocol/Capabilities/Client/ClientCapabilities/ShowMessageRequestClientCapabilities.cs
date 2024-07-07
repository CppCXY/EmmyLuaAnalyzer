using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class ShowMessageRequestClientCapabilities
{
    /**
     * Capabilities specific to the `MessageActionItem` type.
     */
    [JsonPropertyName("messageActionItem")]
    public MessageActionItemClientCapabilities? MessageActionItem { get; init; }
}

public class MessageActionItemClientCapabilities
{
    /**
     * Whether the client supports additional attributes which
     * are preserved and sent back to the server in the request's response.
     */
    [JsonPropertyName("additionalAttributesSupport")]
    public bool? AdditionalAttributesSupport { get; init; }
}
