using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(PositionEncodingKindConverter))]
public readonly record struct PositionEncodingKind(string EncodingKind)
{
    /**
     * Character offsets count UTF-8 code units (i.e. bytes).
     */
    public static readonly PositionEncodingKind UTF8 = new("utf-8");

    /**
     * Character offsets count UTF-16 code units.
     *
     * This is the default and must always be supported
     * by servers.
     */
    public static readonly PositionEncodingKind UTF16 = new("utf-16");

    /**
     * Character offsets count UTF-32 code units.
     *
     * Implementation note: these are the same as Unicode code points,
     * so this `PositionEncodingKind` may also be used for an
     * encoding-agnostic representation of character offsets.
     */
    public static readonly PositionEncodingKind UTF32 = new("utf-32");

    public string EncodingKind { get; } = EncodingKind;
}

public class PositionEncodingKindConverter : JsonConverter<PositionEncodingKind>
{
    public override PositionEncodingKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return new PositionEncodingKind(value!);
    }

    public override void Write(Utf8JsonWriter writer, PositionEncodingKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.EncodingKind);
    }
}