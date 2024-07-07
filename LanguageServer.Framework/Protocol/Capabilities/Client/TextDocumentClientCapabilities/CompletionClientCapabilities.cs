using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class CompletionClientCapabilities
{
    /**
     * Whether completion supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports the following `CompletionItem` specific
     * capabilities.
     */
    [JsonPropertyName("completionItem")]
    public CompletionItemClientCapabilities? CompletionItem { get; init; }

    [JsonPropertyName("completionItemKind")]
    public CompletionItemKindClientCapabilities? CompletionItemKind { get; init; }

    /**
     * The client supports to send additional context information for a
     * `textDocument/completion` request.
     */
    [JsonPropertyName("contextSupport")]
    public bool? ContextSupport { get; init; }

    /**
     * The client's default when the completion item doesn't provide an
     * `insertTextMode` property.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("insertTextModeSupport")]
    public InsertTextModeSupportClientCapabilities? InsertTextMode { get; init; }

    /**
     * The client supports the following `CompletionList` specific
     * capabilities.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("completionList")]
    public CompletionListClientCapabilities? CompletionList { get; init; }
}

public class CompletionItemClientCapabilities
{
    /**
     * Client supports snippets as insert text.
     *
     * A snippet can define tab stops and placeholders with `$1`, `$2`
     * and `${3:foo}`. `$0` defines the final tab stop, it defaults to
     * the end of the snippet. Placeholders with equal identifiers are linked,
     * that is typing in one will update others too.
     */
    [JsonPropertyName("snippetSupport")]
    public bool? SnippetSupport { get; init; }

    /**
     * Client supports commit characters on a completion item.
     */
    [JsonPropertyName("commitCharactersSupport")]
    public bool? CommitCharactersSupport { get; init; }

    /**
     * Client supports the follow content formats for the documentation
     * property. The order describes the preferred format of the client.
     */
    [JsonPropertyName("documentationFormat")]
    public string[]? DocumentationFormat { get; init; }

    /**
     * Client supports the deprecated property on a completion item.
     */
    [JsonPropertyName("deprecatedSupport")]
    public bool? DeprecatedSupport { get; init; }

    /**
     * Client supports the preselect property on a completion item.
     */
    [JsonPropertyName("preselectSupport")]
    public bool? PreselectSupport { get; init; }

    /**
     * Client supports the tag property on a completion item. Clients supporting
     * tags have to handle unknown tags gracefully. Clients especially need to
     * preserve unknown tags when sending a completion item back to the server in
     * a resolve call.
     */
    [JsonPropertyName("tagSupport")]
    public SymbolTagSupportClientCapabilities? TagSupport { get; init; }

    /**
     * Client supports insert replace edit to control different behavior if a
     * completion item is inserted that has the same range as an already inserted
     * completion item.
     */
    [JsonPropertyName("insertReplaceSupport")]
    public bool? InsertReplaceSupport { get; init; }

    /**
     * Indicates which properties a client can resolve lazily on a completion
     * item. Before version 3.16.0 only the predefined properties `documentation`
     * and `details` could be resolved lazily.
     */
    [JsonPropertyName("resolveSupport")]
    public CodeActionResolveSupportClientCapabilities? ResolveSupport { get; init; }

    /**
     * The client supports the `insertTextMode` property on
     * a completion item to override the whitespace handling mode
     * as defined by the client (see `insertTextMode`).
     */
    [JsonPropertyName("insertTextModeSupport")]
    public InsertTextModeSupportClientCapabilities? InsertTextModeSupport { get; init; }


    /**
     * The client has support for completion item label
     * details (see also `CompletionItemLabelDetails`).
     *
     * @since 3.17.0
     */
    [JsonPropertyName("labelDetailsSupport")]
    public bool? LabelDetailsSupport { get; init; }
}

public class CompletionTagSupportClientCapabilities
{
    /**
     * The tags supported by the client.
     */
    [JsonPropertyName("valueSet")]
    public List<string> ValueSet { get; init; } = null!;
}

public class CompletionResolveSupportClientCapabilities
{
    /**
     * The properties that a client can resolve lazily.
     */
    [JsonPropertyName("properties")]
    public List<string> Properties { get; init; } = null!;
}

public class InsertTextModeSupportClientCapabilities
{
    /**
     * The supported insert text modes.
     */
    [JsonPropertyName("valueSet")]
    public List<InsertTextMode> ValueSet { get; init; } = null!;
}

public class CompletionItemKindClientCapabilities
{
    /**
     * The completion item kind values the client supports. When this
     * property exists the client also guarantees that it will
     * handle values outside its set gracefully and falls back
     * to a default value when unknown.
     *
     * If this property is not present the client only supports
     * the completion items kinds from `Text` to `Reference` as defined in
     * the initial version of the protocol.
     */
    [JsonPropertyName("valueSet")]
    public List<CompletionItemKind>? ValueSet { get; init; } = null;
}

public class CompletionListClientCapabilities
{
    /**
     * The client supports the following itemDefaults on
     * a completion list.
     *
     * The value lists the supported property names of the
     * `CompletionList.itemDefaults` object. If omitted,
     * no properties are supported.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("itemDefaults")]
    public List<string>? ItemDefaults { get; init; } = null;
}
