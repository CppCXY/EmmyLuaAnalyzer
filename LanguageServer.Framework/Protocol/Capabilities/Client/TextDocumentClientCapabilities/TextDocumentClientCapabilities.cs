using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class TextDocumentClientCapabilities
{
    /**
     * Synchronization supports.
     */
    [JsonPropertyName("synchronization")]
    public TextDocumentSyncClientCapabilities? Synchronization { get; init; }

    /**
     * Capabilities specific to the `textDocument/completion`
     */
    [JsonPropertyName("completion")]
    public CompletionClientCapabilities? Completion { get; init; }

    /**
     * Capabilities specific to the `textDocument/hover`
     */
    [JsonPropertyName("hover")]
    public HoverClientCapabilities? Hover { get; init; }

    /**
     * Capabilities specific to the `textDocument/signatureHelp`
     */
    [JsonPropertyName("signatureHelp")]
    public SignatureHelpClientCapabilities? SignatureHelp { get; init; }

    /**
     * Capabilities specific to the `textDocument/declaration` request.
     *
     * @since 3.14.0
     */
    [JsonPropertyName("declaration")]
    public DeclarationClientCapabilities? Declaration { get; init; }

    /**
     * Capabilities specific to the `textDocument/definition`
     */
    [JsonPropertyName("definition")]
    public DefinitionClientCapabilities? Definition { get; init; }

    /**
     * Capabilities specific to the `textDocument/typeDefinition`
     */
    [JsonPropertyName("typeDefinition")]
    public TypeDefinitionClientCapabilities? TypeDefinition { get; init; }


    /**
     * Capabilities specific to the `textDocument/implementation`
     */
    [JsonPropertyName("implementation")]
    public ImplementationClientCapabilities? Implementation { get; init; }

    /**
     * Capabilities specific to the `textDocument/references`
     */
    [JsonPropertyName("references")]
    public ReferencesClientCapabilities? References { get; init; }

    /**
     * Capabilities specific to the `textDocument/documentHighlight`
     */
    [JsonPropertyName("documentHighlight")]
    public DocumentHighlightClientCapabilities? DocumentHighlight { get; init; }

    /**
     * Capabilities specific to the `textDocument/documentSymbol`
     */
    [JsonPropertyName("documentSymbol")]
    public DocumentSymbolClientCapabilities? DocumentSymbol { get; init; }


    /**
     * Capabilities specific to the `textDocument/codeAction` request.
     */
    [JsonPropertyName("codeAction")]
    public CodeActionClientCapabilities? CodeAction { get; init; }

    /**
     * Capabilities specific to the `textDocument/codeLens` request.
     */
    [JsonPropertyName("codeLens")]
    public CodeLensClientCapabilities? CodeLens { get; init; }

    /**
     * Capabilities specific to the `textDocument/documentLink` request.
     */
    [JsonPropertyName("documentLink")]
    public DocumentLinkClientCapabilities? DocumentLink { get; init; }

    /**
     * Capabilities specific to the `textDocument/documentColor` and the
     * `textDocument/colorPresentation` request.
     *
     * @since 3.6.0
     */
    [JsonPropertyName("colorProvider")]
    public DocumentColorClientCapabilities? ColorProvider { get; init; }

    /**
     * Capabilities specific to the `textDocument/formatting`
     */
    [JsonPropertyName("formatting")]
    public FormattingClientCapabilities? Formatting { get; init; }

    /**
     * Capabilities specific to the `textDocument/rangeFormatting` and
     * `textDocument/rangesFormatting requests.
     */
    [JsonPropertyName("rangeFormatting")]
    public RangeFormattingClientCapabilities? RangeFormatting { get; init; }

    /**
     * Capabilities specific to the `textDocument/onTypeFormatting`
     */
    [JsonPropertyName("onTypeFormatting")]
    public OnTypeFormattingClientCapabilities? OnTypeFormatting { get; init; }

    /**
     * Capabilities specific to the `textDocument/rename` request.
     */
    [JsonPropertyName("rename")]
    public RenameClientCapabilities? Rename { get; init; }

    /**
     * Capabilities specific to the `textDocument/publishDiagnostics`
     * notification.
     */
    [JsonPropertyName("publishDiagnostics")]
    public PublishDiagnosticsClientCapabilities? PublishDiagnostics { get; init; }

    /**
     * Capabilities specific to the `textDocument/foldingRange` request.
     *
     * @since 3.10.0
     */
    [JsonPropertyName("foldingRange")]
    public FoldingRangeClientCapabilities? FoldingRange { get; init; }

    /**
     * Capabilities specific to the `textDocument/selectionRange` request.
     *
     * @since 3.15.0
     */
    [JsonPropertyName("selectionRange")]
    public SelectionRangeClientCapabilities? SelectionRange { get; init; }

    /**
     * Capabilities specific to the `textDocument/linkedEditingRange` request.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("linkedEditingRange")]
    public LinkedEditingRangeClientCapabilities? LinkedEditingRange { get; init; }

    /**
     * Capabilities specific to the various call hierarchy requests.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("callHierarchy")]
    public CallHierarchyClientCapabilities? CallHierarchy { get; init; }

    /**
     * Capabilities specific to the various semantic token requests.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("semanticTokens")]
    public SemanticTokensClientCapabilities? SemanticTokens { get; init; }

    /**
     * Capabilities specific to the `textDocument/moniker` request.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("moniker")]
    public MonikerClientCapabilities? Moniker { get; init; }

    /**
     * Capabilities specific to the various type hierarchy requests.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("typeHierarchy")]
    public TypeHierarchyClientCapabilities? TypeHierarchy { get; init; }

    /**
     * Capabilities specific to the `textDocument/inlineValue` request.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("inlineValue")]
    public InlineValueClientCapabilities? InlineValue { get; init; }

    /**
     * Capabilities specific to the `textDocument/inlayHint` request.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("inlayHint")]
    public InlayHintClientCapabilities? InlayHint { get; init; }

    /**
     * Capabilities specific to the diagnostic pull model.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("diagnostic")]
    public  DiagnosticClientCapabilities? Diagnostic { get; init; }

    /**
     * Capabilities specific to the `textDocument/inlineCompletion` request.
     *
     * @since 3.18.0
     */
    [JsonPropertyName("inlineCompletion")]
    public InlineCompletionClientCapabilities? InlineCompletion { get; init; }
}
