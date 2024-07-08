using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;

public class TextDocumentServerCapabilities
{
    public class DiagnosticPullModel
    {
        /**
         * Whether the server supports `MarkupContent` in diagnostic messages.
         *
         * @since 3.18.0
         * @proposed
         */
        [JsonPropertyName("markupMessageSupport")]
        public bool? MarkupMessageSupport { get; set; }
    }

    /**
     * Capabilities specific to the diagnostic pull model.
     *
     * @since 3.18.0
     */
    [JsonPropertyName("diagnostic")]
    public DiagnosticPullModel? Diagnostic { get; set; }
}


