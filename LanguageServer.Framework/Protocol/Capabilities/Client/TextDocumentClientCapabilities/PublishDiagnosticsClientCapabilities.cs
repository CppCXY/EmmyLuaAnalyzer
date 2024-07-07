using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class PublishDiagnosticsClientCapabilities
{
    /**
     * Whether the clients accepts diagnostics with related information.
     */
    [JsonPropertyName("relatedInformation")]
    public bool? RelatedInformation { get; init; }

    /**
     * Client supports the tag property to provide meta data about a diagnostic.
     * Clients supporting tags have to handle unknown tags gracefully.
     */
    [JsonPropertyName("tagSupport")]
    public PublishDiagnosticsTagSupportClientCapabilities? TagSupport { get; init; }

    /**
     * Whether the client interprets the version property of the
     * `textDocument/publishDiagnostics` notification's parameter.
     */
    [JsonPropertyName("versionSupport")]
    public bool? VersionSupport { get; init; }

    /**
     * Client supports a codeDescription property on the `Diagnostic` tag which
     * is used to render the code description in the user interface.
     */
    [JsonPropertyName("codeDescriptionSupport")]
    public bool? CodeDescriptionSupport { get; init; }

    /**
     * Whether code action supports the `data` property which is
     * preserved between a `textDocument/publishDiagnostics` and `textDocument/codeAction` request.
     */
    [JsonPropertyName("dataSupport")]
    public bool? DataSupport { get; init; }
}

public class PublishDiagnosticsTagSupportClientCapabilities
{
    /**
     * The tags supported by the client.
     */
    [JsonPropertyName("valueSet")]
    public List<DiagnosticTag> ValueSet { get; init; } = null!;
}
