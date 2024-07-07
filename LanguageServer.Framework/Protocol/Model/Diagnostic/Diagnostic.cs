using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

public class Diagnostic
{
    /**
     * The range at which the message applies.
     */
    [JsonPropertyName("range")]
    public Range Range { get; set; }

    /**
     * The diagnostic's severity.
     */
    [JsonPropertyName("severity")]
    public DiagnosticSeverity? Severity { get; set; }

    /**
     * The diagnostic's code, which might appear in the user interface.
     */
    [JsonPropertyName("code")]
    public StringOrInt? Code { get; set; }

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
    [JsonPropertyName("message")]
    public StringOrMarkupContent Message { get; set; }

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
    public object? Data { get; set; }
}
