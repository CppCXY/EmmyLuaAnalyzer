using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

/**
 * A string value used as a snippet is a template which allows to insert text
 * and to control the editor cursor when insertion happens.
 *
 * A snippet can define tab stops and placeholders with `$1`, `$2`
 * and `${3:foo}`. `$0` defines the final tab stop, it defaults to
 * the end of the snippet. Variables are defined with `$name` and
 * `${name:default value}`.
 *
 * @since 3.18.0
 */
[method: JsonConstructor]
public record struct StringValue(string Kind, string Value)
{
     /**
     * The kind of string value.
     */
     [JsonPropertyName("kind")]
     public string Kind { get; } = Kind;
     
     /**
     * The snippet string.
     */
     [JsonPropertyName("value")]
     public string Value { get; } = Value;
}