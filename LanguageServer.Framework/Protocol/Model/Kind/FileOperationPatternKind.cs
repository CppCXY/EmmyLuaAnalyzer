using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(FileOperationPatternKindConverter))]
public readonly record struct FileOperationPatternKind(string Value)
{
    public static readonly FileOperationPatternKind File = new("file");

    public static readonly FileOperationPatternKind Folder = new("folder");

    public string Value { get; init; } = Value;
}

public class FileOperationPatternKindConverter : JsonConverter<FileOperationPatternKind>
{
    public override FileOperationPatternKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new FileOperationPatternKind(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, FileOperationPatternKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
