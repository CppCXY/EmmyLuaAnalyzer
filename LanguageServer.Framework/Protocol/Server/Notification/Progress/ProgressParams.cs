using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Protocol.Server.Notification.Progress;

[JsonRpc("$/progress")]
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