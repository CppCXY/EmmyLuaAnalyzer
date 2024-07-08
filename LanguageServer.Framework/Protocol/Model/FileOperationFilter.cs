using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

/**
 * A filter to describe in which file operation requests or notifications
 * the server is interested in.
 *
 * @since 3.16.0
 */
public class FileOperationFilter
{
    /**
     * A Uri like `file` or `untitled`.
     */
    [JsonPropertyName("scheme")]
    public string? Scheme { get; set; }

    /**
     * The actual file operation filters.
     */
    [JsonPropertyName("pattern")]
    public FileOperationPattern[]? Pattern { get; set; }
}

public class FileOperationPattern
{
    /**
     * The glob pattern to match. Glob patterns can have the following syntax:
     * - `*` to match one or more characters in a path segment
     * - `?` to match on one character in a path segment
     * - `**` to match any number of path segments, including none
     * - `{}` to group sub patterns into an OR expression. (e.g. `**​/*.{ts,js}`
     *   matches all TypeScript and JavaScript files)
     * - `[]` to declare a range of characters to match in a path segment
     *   (e.g., `example.[0-9]` to match on `example.0`, `example.1`, …)
     * - `[!...]` to negate a range of characters to match in a path segment
     *   (e.g., `example.[!0-9]` to match on `example.a`, `example.b`, but
     *   not `example.0`)
     */
    [JsonPropertyName("glob")]
    public string Glob { get; set; } = string.Empty;

    /**
     * Whether to match files or folders with this pattern.
     *
     * Matches both if undefined.
     */
    [JsonPropertyName("matches")]
    public FileOperationPatternKind? Matches { get; set; }

    /**
     * Additional options used during matching.
     */
    [JsonPropertyName("options")]
    public FileOperationPatternOptions? Options { get; set; }
}

public class FileOperationPatternOptions
{
    /**
     * The pattern should be matched ignoring casing.
     */
    [JsonPropertyName("ignoreCase")]
    public bool? IgnoreCase { get; set; }
}
