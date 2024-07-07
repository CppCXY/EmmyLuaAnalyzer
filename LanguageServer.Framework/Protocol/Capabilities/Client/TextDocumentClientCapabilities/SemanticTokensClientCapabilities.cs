using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class SemanticTokensClientCapabilities
{
    /**
     * Whether implementation supports dynamic registration. If this is set to `true`
     * the client supports the new `(TextDocumentRegistrationOptions & StaticRegistrationOptions)`
     * return value for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Which requests the client supports and might send to the server
     * depending on the server's capability. Please note that clients might not
     * show semantic tokens or degrade some of the user experience if a range
     * or full request is advertised by the client but not provided by the
     * server. If, for example, the client capability `requests.full` and
     * `request.range` are both set to true but the server only provides a
     * range provider, the client might not render a minimap correctly or might
     * even decide to not show any semantic tokens at all.
     */
    [JsonPropertyName("requests")]
    public SemanticTokensClientCapabilitiesRequests? Requests { get; init; }

    /**
     * The token types that can be represented.
     */
    [JsonPropertyName("tokenTypes")]
    public List<string> TokenTypes { get; init; } = null!;

    /**
     * The token modifiers that can be represented.
     */
    [JsonPropertyName("tokenModifiers")]
    public List<string> TokenModifiers { get; init; } = null!;

    /**
     * The formats the client supports.
     */
    [JsonPropertyName("formats")]
    public List<TokenFormat> Formats { get; init; } = null!;

    /**
     * Whether the client supports tokens that can overlap each other.
     */
    [JsonPropertyName("overlappingTokenSupport")]
    public bool? OverlappingTokenSupport { get; init; }

    /**
     * Whether the client supports tokens that can span multiple lines.
     */
    [JsonPropertyName("multilineTokenSupport")]
    public bool? MultilineTokenSupport { get; init; }

    /**
     * Whether the client allows the server to actively cancel a
     * semantic token request, e.g. supports returning
     * ErrorCodes.ServerCancelled. If a server does so, the client
     * needs to retrigger the request.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("serverCancelSupport")]
    public bool? ServerCancelSupport { get; init; }

    /**
     * Whether the client uses semantic tokens to augment existing
     * syntax tokens. If set to `true`, client side created syntax
     * tokens and semantic tokens are both used for colorization. If
     * set to `false`, the client only uses the returned semantic tokens
     * for colorization.
     *
     * If the value is `undefined` then the client behavior is not
     * specified.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("augmentsSyntaxTokens")]
    public bool? AugmentsSyntaxTokens { get; init; }
}

public class SemanticTokensClientCapabilitiesRequests
{
    /**
     * The client will send the `textDocument/semanticTokens/range` request if
     * the server provides a corresponding server capability.
     */
    [JsonPropertyName("range")]
    public bool? Range { get; init; }

    /**
     * The client will send the `textDocument/semanticTokens/full` request if
     * the server provides a corresponding server capability.
     */
    [JsonPropertyName("full"), JsonConverter(typeof(BooleanOrConverter<SemanticTokensClientCapabilitiesRequestsFull>))]
    public BooleanOr<SemanticTokensClientCapabilitiesRequestsFull>? Full { get; init; }
}

public class SemanticTokensClientCapabilitiesRequestsFull
{
    /**
     * The client will send the `textDocument/semanticTokens/full/delta` request if
     * the server provides a corresponding server capability.
     */
    [JsonPropertyName("delta")]
    public bool? Delta { get; init; }
}
