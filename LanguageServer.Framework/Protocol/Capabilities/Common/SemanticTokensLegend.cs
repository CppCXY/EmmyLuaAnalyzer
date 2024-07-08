using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;

public class SemanticTokensLegend
{
    /**
     * The token types a server uses.
     */
    [JsonPropertyName("tokenTypes")]
    public List<string> TokenTypes { get; init; } = [];

    /**
     * The token modifiers a server uses.
     */
    [JsonPropertyName("tokenModifiers")]
    public List<string> TokenModifiers { get; init; } = [];
}
