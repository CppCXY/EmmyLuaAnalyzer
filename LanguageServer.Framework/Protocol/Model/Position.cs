using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

[method: JsonConstructor]
public record struct Position(uint Line, uint Character)
{
    /**
     * Line position in a document (zero-based).
     */
    [JsonPropertyName("line")]
    public uint Line { get; } = Line;
    
    /**
     * Character offset on a line in a document (zero-based). The meaning of this
     * offset is determined by the negotiated `PositionEncodingKind`.
     *
     * If the character value is greater than the line length it defaults back
     * to the line length.
     */
    [JsonPropertyName("character")]
    public uint Character { get; } = Character;
}