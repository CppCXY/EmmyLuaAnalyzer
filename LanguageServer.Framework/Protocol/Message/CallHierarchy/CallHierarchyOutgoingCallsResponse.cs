using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

[JsonConverter(typeof(CallHierarchyOutgoingCallsResponseJsonConverter))]
public class CallHierarchyOutgoingCallsResponse(List<CallHierarchyOutgoingCall> result)
{
    public List<CallHierarchyOutgoingCall> Result { get; set; } = result;
}

public class CallHierarchyOutgoingCallsResponseJsonConverter : JsonConverter<CallHierarchyOutgoingCallsResponse>
{
    public override CallHierarchyOutgoingCallsResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new UnreachableException();
    }

    public override void Write(Utf8JsonWriter writer, CallHierarchyOutgoingCallsResponse value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Result, options);
    }
}
