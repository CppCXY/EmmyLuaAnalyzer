using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class SemanticTokensOptions : WorkDoneProgressOptions
{
    /**
     * The legend used by the server.
     */
    [JsonPropertyName("legend")]
    public SemanticTokensLegend Legend { get; init; } = new();

    /**
     * Server supports providing semantic tokens for a specific range
     * of a document.
     */
    [JsonPropertyName("range")]
    public bool? Range { get; init; }

    /**
     * Server supports providing semantic tokens for a full document.
     */
    [JsonPropertyName("full"), JsonConverter(typeof(SemanticTokensCapabilitiesFull))]
    public BooleanOr<SemanticTokensCapabilitiesFull>? Full { get; init; }
}
