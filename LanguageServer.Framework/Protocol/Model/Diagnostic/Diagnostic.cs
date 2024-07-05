using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

public class Diagnostic(Range range, OneOf2<string, MarkupContent> message)
{
    /**
     * The range at which the message applies.
     */
    [JsonPropertyName("range")]
    public Range Range { get; set; } = range;

    /**
     * The diagnostic's severity.
     */
    [JsonPropertyName("severity")]
    public DiagnosticSeverity? Severity { get; set; }

    /**
     * The diagnostic's code, which might appear in the user interface.
     */
    [JsonPropertyName("code"), JsonConverter(typeof(OneOf2JsonConverter<string, int>))]
    public OneOf2<string, int>? Code { get; set; }

    /**
     * An optional property to describe the error code.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("codeDescription")]
    public CodeDescription? CodeDescription { get; set; }

    /**
     * A human-readable string describing the source of this
     * diagnostic, e.g. 'typescript' or 'super lint'.
     */
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /**
     * The diagnostic's message.
     */
    [JsonPropertyName("message"), JsonConverter(typeof(OneOf2JsonConverter<string, MarkupContent>))]
    public OneOf2<string, MarkupContent> Message { get; set; } = message;

    /**
     * An array of related diagnostic information, e.g. when symbol-names within
     * a scope collide all definitions can be marked via this property.
     */
    [JsonPropertyName("tags")]
    public List<DiagnosticTag>? Tags { get; set; }
    
    /**
     * An array of related diagnostic information, e.g. when symbol-names within
     * a scope collide all definitions can be marked via this property.
     */
    [JsonPropertyName("relatedInformation")]
    public List<DiagnosticRelatedInformation>? RelatedInformation { get; set; }
    
    /**
     * A data entry field that is preserved between a
     * `textDocument/publishDiagnostics` notification and
     * `textDocument/codeAction` request.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("data")]
    public Object? Data { get; set; }
}