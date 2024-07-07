using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

namespace EmmyLua.LanguageServer.Framework.Protocol.Notification;

[JsonRpc("$/cancelRequest")]
public class CancelParams
{
    [JsonPropertyName("id"), JsonConverter(typeof(OneOf2JsonConverter<int, string>))]
    public OneOf<int, string> Id { get; } = null!;
}
