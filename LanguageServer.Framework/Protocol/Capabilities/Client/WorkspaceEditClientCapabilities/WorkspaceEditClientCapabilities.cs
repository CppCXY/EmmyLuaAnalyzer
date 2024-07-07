using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.WorkspaceEditClientCapabilities;

public class WorkspaceEditClientCapabilities
{
    /**
     * The client supports versioned document changes in `WorkspaceEdit`s
     */
    [JsonPropertyName("documentChanges")]
    public bool? DocumentChanges { get; set; }

    /**
     * The resource operations the client supports. Clients should at least
     * support 'create', 'rename', and 'delete' for files and folders.
     *
     * @since 3.13.0
     */
    [JsonPropertyName("resourceOperations")]
    public List<ResourceOperationKind>? ResourceOperations { get; set; }

    /**
     * The failure handling strategy of a client if applying the workspace edit
     * fails.
     *
     * @since 3.13.0
     */
    [JsonPropertyName("failureHandling")]
    public FailureHandlingKind? FailureHandling { get; set; }

    /**
     * Whether the client normalizes line endings to the client specific
     * setting.
     * If set to `true`, the client will normalize line ending characters
     * in a workspace edit to the client specific new line character(s).
     *
     * @since 3.16.0
     */
    [JsonPropertyName("normalizesLineEndings")]
    public bool? NormalizesLineEndings { get; set; }

    /**
     * Whether the client in general supports change annotations on text edits,
     * create file, rename file and delete file changes.
     */
    [JsonPropertyName("changeAnnotationSupport")]
    public ChangeAnnotationSupport? ChangeAnnotationSupport { get; set; }

    /**
     * Whether the client supports `WorkspaceEditMetadata` in `WorkspaceEdit`s.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("metadataSupport")]
    public bool? MetadataSupport { get; set; }
    
    /**
     * Whether the client supports snippets as text edits.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("snippetSupport")]
    public bool? SnippetSupport { get; set; }
}