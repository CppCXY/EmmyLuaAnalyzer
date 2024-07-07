using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.File;

/**
 * Rename file options
 */
[method: JsonConstructor]
public readonly record struct RenameFileOptions(bool? Overwrite, bool? IgnoreIfExists)
{
    /**
     * Overwrite target if existing. Overwrite wins over `ignoreIfExists`.
     */
    public bool? Overwrite { get; } = Overwrite;

    /**
     * Ignore if target exists.
     */
    public bool? IgnoreIfExists { get; } = IgnoreIfExists;
}

/**
 * Rename file operation
 */
[method: JsonConstructor]
public record RenameFile(
    DocumentUri OldUri,
    DocumentUri NewUri,
    RenameFileOptions? Options,
    ChangeAnnotationIdentifier? AnnotationId)
{
    /**
     * The kind of the rename request.
     */
    [JsonPropertyName("kind")]
    public string Kind { get; } = "rename";

    /**
     * The old (existing) location.
     */
    [JsonPropertyName("oldUri")]
    public DocumentUri OldUri { get; } = OldUri;

    /**
     * The new location.
     */
    [JsonPropertyName("newUri")]
    public DocumentUri NewUri { get; } = NewUri;

    /**
     * Additional options.
     */
    [JsonPropertyName("options")]
    public RenameFileOptions? Options { get; } = Options;

    /**
    * An optional annotation identifier describing the operation.
    *
    * @since 3.16.0
    */
    [JsonPropertyName("annotationId")]
    public ChangeAnnotationIdentifier? AnnotationId { get; } = AnnotationId;
}