using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;

public class SemanticTokensCapabilitiesFull
{
    /**
     * The client will send the `textDocument/semanticTokens/full/delta` request if
     * the server provides a corresponding server capability.
     */
    [JsonPropertyName("delta")]
    public bool? Delta { get; init; }
}
