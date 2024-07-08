using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class CodeLensOptions : WorkDoneProgressOptions
{
    /**
     * Code lens has a resolve provider as well.
     */
    [JsonPropertyName("resolveProvider")]
    public bool? ResolveProvider { get; set; }
}
