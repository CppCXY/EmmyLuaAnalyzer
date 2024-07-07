using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class DeclarationClientCapabilities
{
    /**
     * Whether declaration supports dynamic registration. If this is set to `true`
     * the client supports the new `DeclarationRegistrationOptions` return value
     * for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports additional metadata in the form of declaration links.
     */
    [JsonPropertyName("linkSupport")]
    public bool? LinkSupport { get; init; }
}
