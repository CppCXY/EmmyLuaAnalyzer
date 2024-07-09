using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Progress;

public class ProgressParams
{
    /**
     * The progress token provided by the client or server.
     */
    [JsonPropertyName("token")]
    public string Token { get; set; } = null!;

    /**
     * The progress data.
     */
    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;
}
