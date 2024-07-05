using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

[JsonConverter(typeof(DiagnosticSeverityJsonConverter))]
public readonly record struct DiagnosticSeverity(int Value)
{
    /**
     * Reports an error.
     */
    public static DiagnosticSeverity Error { get; } = new DiagnosticSeverity(1);
    
    /**
     * Reports a warning.
     */
    public static DiagnosticSeverity Warning { get; } = new DiagnosticSeverity(2);
    
    /**
     * Reports an information.
     */
    public static DiagnosticSeverity Information { get; } = new DiagnosticSeverity(3);
    
    /**
     * Reports a hint.
     */
    public static DiagnosticSeverity Hint { get; } = new DiagnosticSeverity(4);
    
    public int Value { get; } = Value;
}

public class DiagnosticSeverityJsonConverter : JsonConverter<DiagnosticSeverity>
{
    public override DiagnosticSeverity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new DiagnosticSeverity(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, DiagnosticSeverity value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}