using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

[JsonConverter(typeof(CallHierarchyIncomingCallsResponseJsonConverter))]
public class CallHierarchyIncomingCallsResponse(List<CallHierarchyIncomingCall> result)
{
    public List<CallHierarchyIncomingCall> Result { get; set; } = result;
}

public class CallHierarchyIncomingCallsResponseJsonConverter : JsonConverter<CallHierarchyIncomingCallsResponse>
{
    public override CallHierarchyIncomingCallsResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new UnreachableException();
    }

    public override void Write(Utf8JsonWriter writer, CallHierarchyIncomingCallsResponse value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Result, options);
    }
}
