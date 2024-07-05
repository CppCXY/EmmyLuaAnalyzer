using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Server.Request.Initialize;

public class ClientInfo
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")] public string? Version { get; set; }
}