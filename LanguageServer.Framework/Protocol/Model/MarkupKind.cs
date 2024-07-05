using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

/**
 * Describes the content type that a client supports in various
 * result literals like `Hover`, `ParameterInfo` or `CompletionItem`.
 *
 * Please note that `MarkupKinds` must not start with a `$`. These kinds
 * are reserved for internal usage.
 */
[JsonConverter(typeof(MarkupKindJsonConverter))]
public readonly record struct MarkupKind(string Value)
{
    /**
     * Plain text is supported as a content format
     */
    public static MarkupKind PlainText { get; } = new MarkupKind("plaintext");

    /**
     * Markdown is supported as a content format
     */
    public static MarkupKind Markdown { get; } = new MarkupKind("markdown");

    public string Value { get; } = Value;
}

public class MarkupKindJsonConverter : JsonConverter<MarkupKind>
{
    public override MarkupKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new MarkupKind(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, MarkupKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}