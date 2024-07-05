using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

DiagnosticSeverity

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
    public DiagnosticSeverity? Severity { get; set; }

    /**
     * The diagnostic's code, which might appear in the user interface.
     */
    public string Code { get; set; }

    /**
     * A human-readable string describing the source of this
     * diagnostic, e.g. 'typescript' or 'super lint'.
     */
    public string Source { get; set; }

    /**
     * The diagnostic's message.
     */
    public string Message { get; set; }

    /**
     * An array of related diagnostic information, e.g. when symbol-names within
     * a scope collide all definitions can be marked via this property.
     */
    public Container<DiagnosticRelatedInformation> RelatedInformation { get; set; }
}