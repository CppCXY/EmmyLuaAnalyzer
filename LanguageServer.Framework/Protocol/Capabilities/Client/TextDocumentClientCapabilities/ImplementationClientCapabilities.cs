using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class ImplementationClientCapabilities
{
    /**
     * Whether implementation supports dynamic registration. If this is set to `true`
     * the client supports the new `ImplementationRegistrationOptions` return value
     * for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports additional metadata in the form of implementation links.
     */
    [JsonPropertyName("linkSupport")]
    public bool? LinkSupport { get; init; }
}
