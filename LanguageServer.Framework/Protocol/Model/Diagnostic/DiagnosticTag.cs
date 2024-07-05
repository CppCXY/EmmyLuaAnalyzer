using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

[JsonConverter(typeof(DiagnosticTagJsonConverter))]
public record struct DiagnosticTag(int Value)
{
    /**
     * Unused or unnecessary code.
     *
     * Clients are allowed to render diagnostics with this tag faded out
     * instead of having an error squiggle.
     */
    public static readonly DiagnosticTag Unnecessary = new(1);
    
    /**
     * Deprecated or obsolete code.
     *
     * Clients are allowed to rendered diagnostics with this tag strike through.
     */
    public static readonly DiagnosticTag Deprecated = new(2);
    
    public int Value { get; } = Value;
}

public class DiagnosticTagJsonConverter : JsonConverter<DiagnosticTag>
{
    public override DiagnosticTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new DiagnosticTag(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, DiagnosticTag value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}