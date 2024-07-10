using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using Range = System.Range;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

public class CallHierarchyOutgoingCall
{
    /**
     * The item that is called.
     */
    [JsonPropertyName("to")]
    public Location To { get; set; }

    /**
     * The range at which this item is called. This is the range relative to the caller, e.g the item passed
     * to [`provideCallHierarchyOutgoingCalls`](#CallHierarchyItemProvider.provideCallHierarchyOutgoingCalls) and not
     * [`this.to`](#CallHierarchyOutgoingCall.to).
     */
    [JsonPropertyName("fromRanges")]
    public Range FromRanges { get; set; }
}
