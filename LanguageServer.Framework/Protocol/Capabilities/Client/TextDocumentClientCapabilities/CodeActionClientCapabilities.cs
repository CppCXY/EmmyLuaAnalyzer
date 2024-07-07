using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class CodeActionClientCapabilities
{
    /**
     * Whether code action supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client support code action literals as a valid
     * response of the `textDocument/codeAction` request.
     *
     * @since 3.8.0
     */
    [JsonPropertyName("codeActionLiteralSupport")]
    public CodeActionLiteralSupportClientCapabilities? CodeActionLiteralSupport { get; init; }

    /**
     * Whether code action supports the `isPreferred` property.
     *
     * @since 3.15.0
     */
    [JsonPropertyName("isPreferredSupport")]
    public bool? IsPreferredSupport { get; init; }

    /**
     * Whether code action supports the `disabled` property.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("disabledSupport")]
    public bool? DisabledSupport { get; init; }

    /**
     * Whether code action supports the `data` property which is
     * preserved between a `textDocument/codeAction` and a
     * `codeAction/resolve` request.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("dataSupport")]
    public bool? DataSupport { get; init; }

    /**
     * Whether the client supports resolving additional code action
     * properties via a separate `codeAction/resolve` request.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("resolveSupport")]
    public CodeActionResolveSupportClientCapabilities? ResolveSupport { get; init; }

    /**
     * Whether the client honors the change annotations in
     * text edits and resource operations returned via the
     * `CodeAction#edit` property by, for example, presenting
     * the workspace edit in the user interface and asking
     * for confirmation.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("honorsChangeAnnotations")]
    public bool? HonorsChangeAnnotations { get; init; }

    /**
     * Whether the client supports documentation for a class of code actions.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("documentationSupport")]
    public bool? DocumentationSupport { get; init; }
}

public class CodeActionLiteralSupportClientCapabilities
{
    /**
     * The code action kind is support with the following value
     * set.
     */
    [JsonPropertyName("codeActionKind")]
    public CodeActionKindClientCapabilities CodeActionKind { get; init; }
}

public class CodeActionKindClientCapabilities
{
    /**
     * The code action kind values the client supports. When this
     * property exists the client also guarantees that it will
     * handle values outside its set gracefully and falls back
     * to a default value when unknown.
     */
    [JsonPropertyName("valueSet")]
    public List<CodeActionKind> ValueSet { get; init; } = null!;
}

public class CodeActionResolveSupportClientCapabilities
{
    /**
     * The properties that a client can resolve lazily.
     */
    [JsonPropertyName("properties")]
    public List<string>? Properties { get; init; }
}
