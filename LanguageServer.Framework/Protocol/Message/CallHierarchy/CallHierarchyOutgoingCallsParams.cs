using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Interface;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

public class CallHierarchyOutgoingCallsParams : IWorkDoneProgressParams, IPartialResultParams
{
    [JsonPropertyName("workDoneToken")]
    public string? WorkDoneToken { get; set; }

    [JsonPropertyName("partialResultToken")]
    public string? PartialResultToken { get; set; }

    [JsonPropertyName("item")]
    public CallHierarchyItem Item { get; set; } = null!;
}
