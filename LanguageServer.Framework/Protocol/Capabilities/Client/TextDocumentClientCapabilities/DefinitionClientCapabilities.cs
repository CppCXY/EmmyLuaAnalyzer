using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class DefinitionClientCapabilities
{
    /**
     * Whether definition supports dynamic registration. If this is set to `true`
     * the client supports the new `DefinitionRegistrationOptions` return value
     * for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports additional metadata in the form of definition links.
     */
    [JsonPropertyName("linkSupport")]
    public bool? LinkSupport { get; init; }
}
