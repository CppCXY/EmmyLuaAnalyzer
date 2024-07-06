using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client;

public class MarkdownClientCapabilities(string parser)
{
 /**
     * The name of the parser.
     */
    [JsonPropertyName("parser")]
    public string Parser { get; set; } = parser;

 /**
     * The version of the parser.
     */
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /**
     * A list of HTML tags that the client allows / supports in
     * Markdown.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("allowedTags")]
    public string[]? AllowedTags { get; set; }
}