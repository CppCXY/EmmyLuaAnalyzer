using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class DiagnosticClientCapabilities
{
    /**
     * Whether implementation supports dynamic registration. If this is set to
     * `true`, the client supports the new
     * `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
     * return value for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Whether the clients supports related documents for document diagnostic
     * pulls.
     */
    [JsonPropertyName("relatedInformation")]
    public bool? RelatedInformation { get; init; }

    /**
     * Whether the client supports `MarkupContent` in diagnostic messages.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("markupMessageSupport")]
    public bool? MarkupMessageSupport { get; init; }
}
