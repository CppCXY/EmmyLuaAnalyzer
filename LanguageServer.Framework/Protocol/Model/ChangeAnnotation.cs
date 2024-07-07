using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

/**
 * Additional information that describes document changes.
 *
 * @since 3.16.0
 */
public record ChangeAnnotation(string Label, bool? NeedsConfirmation, string Description)
{
    /**
     * A human-readable string describing the annotation.
     */
    [JsonPropertyName("label")]
    public string Label { get; } = Label;
    
    /**
     * A flag which indicates that user confirmation is needed before applying the change.
     */
    [JsonPropertyName("needsConfirmation")]
    public bool? NeedsConfirmation { get; } = NeedsConfirmation;
    
    /**
     * A human-readable string which explains the change.
     */
    [JsonPropertyName("description")]
    public string Description { get; } = Description;
}


/**
 * An identifier referring to a change annotation managed by a workspace
 * edit.
 *
 * @since 3.16.0.
 */
[JsonConverter(typeof(ChangeAnnotationIdentifierConverter))]
public readonly record struct ChangeAnnotationIdentifier(string Value)
{
    public string Value { get; } = Value;
}

public class ChangeAnnotationIdentifierConverter : JsonConverter<ChangeAnnotationIdentifier>
{
    public override ChangeAnnotationIdentifier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new ChangeAnnotationIdentifier(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, ChangeAnnotationIdentifier value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}