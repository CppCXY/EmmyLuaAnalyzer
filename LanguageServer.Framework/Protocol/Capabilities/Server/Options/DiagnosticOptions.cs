using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class DiagnosticOptions : WorkDoneProgressOptions
{
    /**
     * An optional identifier under which the diagnostics are
     * managed by the client.
     */
    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }

    /**
     * Whether the language has inter file dependencies, meaning that
     * editing code in one file can result in a different diagnostic
     * set in another file. Inter file dependencies are common for
     * most programming languages and typically uncommon for linters.
     */
    [JsonPropertyName("interFileDependencies")]
    public bool InterFileDependencies { get; set; }

    /**
     * The server provides support for workspace diagnostics as well.
     */
    [JsonPropertyName("workspaceDiagnostics")]
    public bool WorkspaceDiagnostics { get; set; }
}
