using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

/**
 * A special text edit with an additional change annotation.
 *
 * @since 3.16.0.
 */
public record AnnotatedTextEdit(string AnnotationId, Range Range, string NewText) : TextEdit(Range, NewText)
{
    /**
     * A unique identifier for the annotated text edit. This is used to
     * address the annotated text edit in a workspace edit.
     */
    [JsonPropertyName("annotationId")]
    public string AnnotationId { get; } = AnnotationId;
}