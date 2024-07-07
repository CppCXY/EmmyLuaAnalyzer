using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class SignatureHelpClientCapabilities
{
    /**
     * Whether signature help supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports the following `SignatureInformation` specific
     * capabilities.
     */
    [JsonPropertyName("signatureInformation")]
    public SignatureInformationClientCapabilities? SignatureInformation { get; init; }

    /**
     * The client supports sending additional context information for a
     * `textDocument/signatureHelp` request. A client that opts into
     * contextSupport will also support the `retriggerCharacters` on
     * `SignatureHelpOptions`.
     *
     * @since 3.15.0
     */
    [JsonPropertyName("contextSupport")]
    public bool? ContextSupport { get; init; }
}

public class SignatureInformationClientCapabilities
{
    /**
     * Client supports snippets as insert text.
     *
     * A snippet can define tab stops and placeholders with `$1`, `$2`
     * and `${3:foo}`. `$0` defines the final tab stop, it defaults to
     * the end of the snippet. Placeholders with equal identifiers are linked,
     * that is typing in one will update others too.
     */
    [JsonPropertyName("documentationFormat")]
    public List<MarkupKind>? DocumentationFormat { get; init; }

    /**
     * Client capabilities specific to parameter information.
     */
    [JsonPropertyName("parameterInformation")]
    public ParameterInformationClientCapabilities? ParameterInformation { get; init; }

    /**
     * The client supports the `activeParameter` property on
     * `SignatureInformation` literal.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("activeParameterSupport")]
    public bool? ActiveParameterSupport { get; init; }

    /**
     * The client supports the `activeParameter` property on
     * `SignatureHelp`/`SignatureInformation` being set to `null` to
     * indicate that no parameter should be active.
     *
     * @since 3.18.0
     */
    [JsonPropertyName("noActiveParameterSupport")]
    public bool? NoActiveParameterSupport { get; init; }
}

public class ParameterInformationClientCapabilities
{
    /**
     * The client supports processing label offsets instead of a
     * simple label string.
     *
     * @since 3.14.0
     */
    [JsonPropertyName("labelOffsetSupport")]
    public bool? LabelOffsetSupport { get; init; }
}
