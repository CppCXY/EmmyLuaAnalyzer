using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

[method: JsonConstructor]
public record struct Range(Position Start, Position End)
{
    /**
     * The range's start position.
     */
    [JsonPropertyName("start")]
    public Position Start { get; } = Start;
    
    /**
     * The range's end position.
     */
    [JsonPropertyName("end")]
    public Position End { get; } = End;
    
    public static Range From(Position start, Position end) => new Range(start, end);
    public static Range From((Position start, Position end) tuple) => new Range(tuple.start, tuple.end);
    
    public static implicit operator Range((Position start, Position end) tuple) => From(tuple);
    
    public static implicit operator (Position start, Position end)(Range range) => (range.Start, range.End);
}
