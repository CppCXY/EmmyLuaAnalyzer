using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

public record struct Location(DocumentUri Uri, Range Range)
{
    /**
     * The URI of the document.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; } = Uri;

    /**
     * The range in side the document.
     */
    [JsonPropertyName("range")]
    public Range Range { get; } = Range;
}
