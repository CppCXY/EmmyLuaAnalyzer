using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.WorkspaceEditClientCapabilities;

[JsonConverter(typeof(ResourceOperationKindConverter))]
public readonly record struct ResourceOperationKind(string Value)
{
    public static ResourceOperationKind Create = new("create");
    
    public static ResourceOperationKind Rename = new("rename");
    
    public static ResourceOperationKind Delete = new("delete");
    
    public string Value { get; } = Value;
}

public class ResourceOperationKindConverter : JsonConverter<ResourceOperationKind>
{
    public override ResourceOperationKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new ResourceOperationKind(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, ResourceOperationKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}