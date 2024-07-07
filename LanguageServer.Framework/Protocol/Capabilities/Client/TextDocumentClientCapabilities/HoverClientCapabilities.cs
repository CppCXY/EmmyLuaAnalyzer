using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class HoverClientCapabilities
{
    /**
     * Whether hover supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Client supports the follow content formats for the content
     * property. The order describes the preferred format of the client.
     */
    [JsonPropertyName("contentFormat")]
    public List<MarkupKind>? ContentFormat { get; init; }
}
