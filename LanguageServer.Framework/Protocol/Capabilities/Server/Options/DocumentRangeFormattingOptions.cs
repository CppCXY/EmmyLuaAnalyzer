using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class DocumentRangeFormattingOptions : WorkDoneProgressOptions
{
    /**
     * Whether the server supports formatting multiple ranges at once.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("rangesSupport")]
    public bool? RangesSupport { get; set; }
}
