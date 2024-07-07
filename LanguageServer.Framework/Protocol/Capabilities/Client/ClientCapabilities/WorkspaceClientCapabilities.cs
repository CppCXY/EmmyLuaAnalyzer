using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class WorkspaceClientCapabilities
{
    /**
     * The client supports applying batch edits to the workspace by supporting
     * the request 'workspace/applyEdit'
     */
    [JsonPropertyName("applyEdit")]
    public bool? ApplyEdit { get; init; }

    /**
     * Capabilities specific to `WorkspaceEdit`s
     */
    [JsonPropertyName("workspaceEdit")]
    public WorkspaceEditClientCapabilities.WorkspaceEditClientCapabilities? WorkspaceEdit { get; init; }

    /**
     * Capabilities specific to the `didChangeConfiguration` notification.
     */
    [JsonPropertyName("didChangeConfiguration")]
    public DidChangeConfigurationClientCapabilities? DidChangeConfiguration { get; init; }

    /**
     * Capabilities specific to the `didChangeWatchedFiles` notification.
     */
    [JsonPropertyName("didChangeWatchedFiles")]
    public DidChangeWatchedFilesClientCapabilities? DidChangeWatchedFiles { get; init; }

    /**
     * Capabilities specific to the `symbol` request.
     */
    [JsonPropertyName("symbol")]
    public WorkspaceSymbolClientCapabilities? Symbol { get; init; }

    /**
     * Capabilities specific to the `executeCommand` request.
     */
    [JsonPropertyName("executeCommand")]
    public ExecuteCommandClientCapabilities? ExecuteCommand { get; init; }

    /**
     * The client has support for workspace folders.
     */
    [JsonPropertyName("workspaceFolders")]
    public bool? WorkspaceFolders { get; init; }

    /**
     * The client supports `workspace/configuration` requests.
     */
    [JsonPropertyName("configuration")]
    public bool? Configuration { get; init; }

    /**
     * Capabilities specific to the semantic token requests scoped to the
     * workspace.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("semanticTokens")]
    public SemanticTokensWorkspaceClientCapabilities? SemanticTokens { get; init; }

    /**
     * Capabilities specific to the code lens requests scoped to the
     * workspace.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("codeLens")]
    public CodeLensWorkspaceClientCapabilities? CodeLens { get; init; }

    /**
     * The client has support for file requests/notifications.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("fileOperations")]
    public FileOperationClientCapabilities? FileOperations { get; init; }

    /**
     * Client workspace capabilities specific to inline values.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("inlineValue")]
    public InlineValueWorkspaceClientCapabilities? InlineValue { get; init; }

    /**
     * Client workspace capabilities specific to inlay hints.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("inlayHint")]
    public InlayHintWorkspaceClientCapabilities? InlayHint { get; init; }

    /**
     * Client workspace capabilities specific to diagnostics.
     *
     * @since 3.17.0.
     */
    [JsonPropertyName("diagnostics")]
    public DiagnosticsWorkspaceClientCapabilities? Diagnostics { get; init; }
}

public class FileOperationClientCapabilities
{
    /**
     * Whether the client supports dynamic registration for file
     * requests/notifications.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client is interested in receiving didCreateFiles notifications.
     */
    [JsonPropertyName("didCreate")]
    public bool? DidCreate { get; init; }

    /**
     * The client is interested in receiving willCreateFiles notifications.
     */
    [JsonPropertyName("willCreate")]
    public bool? WillCreate { get; init; }

    /**
     * The client is interested in receiving didRenameFiles notifications.
     */
    [JsonPropertyName("didRename")]
    public bool? DidRename { get; init; }

    /**
     * The client is interested in receiving willRenameFiles notifications.
     */
    [JsonPropertyName("willRename")]
    public bool? WillRename { get; init; }

    /**
     * The client is interested in receiving didDeleteFiles notifications.
     */
    [JsonPropertyName("didDelete")]
    public bool? DidDelete { get; init; }

    /**
     * The client is interested in receiving willDeleteFiles notifications.
     */
    [JsonPropertyName("willDelete")]
    public bool? WillDelete { get; init; }
}
