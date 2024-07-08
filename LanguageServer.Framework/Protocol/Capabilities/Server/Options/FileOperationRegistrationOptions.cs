using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

/**
 * The options to register for file operations.
 *
 * @since 3.16.0
 */
public class FileOperationRegistrationOptions
{
    /**
     * The actual filters.
     */
    [JsonPropertyName("filters")]
    public List<FileOperationFilter> Filters { get; set; } = [];
}
