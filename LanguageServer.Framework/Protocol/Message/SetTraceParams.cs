using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message;

public class SetTraceParams
{
    /**
     * The trace setting. If omitted, the trace configuration is unchanged.
     */
    [JsonPropertyName("value")]
    public TraceValue Value { get; set; }
}
