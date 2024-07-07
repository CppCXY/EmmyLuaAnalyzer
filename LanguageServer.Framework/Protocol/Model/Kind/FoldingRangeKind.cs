using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

/**
 * Enum of known range kinds
 */
[JsonConverter(typeof(FoldingRangeKindJsonConverter))]
public readonly record struct FoldingRangeKind(string Value)
{
    /**
     * Folding range for a comment
     */
    public static readonly FoldingRangeKind Comment = new("comment");

    /**
     * Folding range for a imports or includes
     */
    public static readonly FoldingRangeKind Imports = new("imports");

    /**
     * Folding range for a region (e.g. `#region`)
     */
    public static readonly FoldingRangeKind Region = new("region");

    public string Value { get; } = Value;
}

public class FoldingRangeKindJsonConverter : JsonConverter<FoldingRangeKind>
{
    public override FoldingRangeKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return new FoldingRangeKind(value!);
    }

    public override void Write(Utf8JsonWriter writer, FoldingRangeKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
