using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message;

public class CancelParams
{
    [JsonPropertyName("id")]
    public StringOrInt Id { get; } = 0;
}
