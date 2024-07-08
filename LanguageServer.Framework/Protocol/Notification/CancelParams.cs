using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Notification;

public class CancelParams
{
    [JsonPropertyName("id")]
    public StringOrInt Id { get; } = 0;
}
