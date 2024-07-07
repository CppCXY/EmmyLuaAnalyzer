using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.File;

/**
 * Options to create a file.
 */
[method:JsonConstructor]
public readonly record struct CreateFileOptions(bool? Overwrite, bool? IgnoreIfExists)
{
    /**
    * Overwrite existing file. Overwrite wins over `ignoreIfExists`.
    */
    [JsonPropertyName("overwrite")]
    public bool? Overwrite { get; } = Overwrite;

    /**
    * Ignore if exists.
    */
    [JsonPropertyName("ignoreIfExists")]
    public bool? IgnoreIfExists { get; } = IgnoreIfExists;
}

/**
 * Create file operation
 */
[method:JsonConstructor]
public record CreateFile(DocumentUri Uri, CreateFileOptions? Options, ChangeAnnotationIdentifier? AnnotationId)
{
    /**
     * The kind of the create request.
     */
    [JsonPropertyName("kind")]
    public string Kind { get; } = "create";
    /**
     * The resource to create.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; } = Uri;

    /**
     * Additional options
     */
    [JsonPropertyName("options")]
    public CreateFileOptions? Options { get; } = Options;

    /**
     * A human-readable string describing the annotation.
     */
    [JsonPropertyName("annotationId")]
    public ChangeAnnotationIdentifier? AnnotationId { get; } = AnnotationId;
}