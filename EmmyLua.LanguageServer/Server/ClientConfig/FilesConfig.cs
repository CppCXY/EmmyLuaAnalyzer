using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Server.ClientConfig;

public class FilesConfig
{
    [JsonPropertyName("exclude")]
    public Dictionary<string, bool> Exclude { get; set; } = new();

    [JsonPropertyName("associations")]
    public Dictionary<string, string> Associations { get; set; } = new();

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = string.Empty;
}