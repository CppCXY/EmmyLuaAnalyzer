using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class DocumentSymbolOptions : WorkDoneProgressOptions
{
    /**
     * A human-readable string that is shown when multiple outlines trees
     * are shown for the same document.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
