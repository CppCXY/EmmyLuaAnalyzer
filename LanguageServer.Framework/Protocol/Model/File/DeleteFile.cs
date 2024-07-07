using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.File;

/**
 * Delete file options
 */
[method: JsonConstructor]
public readonly record struct DeleteFileOptions(bool? Recursive, bool? IgnoreIfNotExists)
{
    /**
     * Delete the content recursively if a folder is denoted.
     */
    [JsonPropertyName("recursive")]
    public bool? Recursive { get; } = Recursive;
    
    /**
     * Ignore if file does not exist.
     */
    [JsonPropertyName("ignoreIfNotExists")]
    public bool? IgnoreIfNotExists { get; } = IgnoreIfNotExists;
}

/**
 * Delete file operation
 */
[method: JsonConstructor]
public record DeleteFile(DocumentUri Uri, DeleteFileOptions? Options, ChangeAnnotationIdentifier? AnnotationId)
{
    /**
     * The kind of the delete request.
     */
    [JsonPropertyName("kind")]
    public string Kind { get; } = "delete";
    
    /**
     * The resource to delete.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; } = Uri;

    /**
     * Additional options
     */
    [JsonPropertyName("options")]
    public DeleteFileOptions? Options { get; } = Options;

    /**
     * An optional annotation identifier describing the operation.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("annotationId")]
    public ChangeAnnotationIdentifier? AnnotationId { get; } = AnnotationId;
}