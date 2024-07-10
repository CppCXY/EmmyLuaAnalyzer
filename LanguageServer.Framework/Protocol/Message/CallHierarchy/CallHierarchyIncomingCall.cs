using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using Range = EmmyLua.LanguageServer.Framework.Protocol.Model.Range;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

public class CallHierarchyIncomingCall
{
    /**
     * The item that makes the call.
     */
    [JsonPropertyName("from")]
    public Location From { get; set; }

    /**
     * The ranges at which the calls appear. This is relative to the caller
     * denoted by [`this.from`](#CallHierarchyIncomingCall.from).
     */
    [JsonPropertyName("fromRanges")]
    public Range FromRanges { get; set; }
}
