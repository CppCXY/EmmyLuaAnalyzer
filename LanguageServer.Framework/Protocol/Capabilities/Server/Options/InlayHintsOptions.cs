using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

/**
 * Inlay hint options used during static registration.
 *
 * @since 3.17.0
 */
public class InlayHintsOptions : WorkDoneProgressOptions
{
    /**
     * The server provides support to resolve additional
     * information for an inlay hint item.
     */
    [JsonPropertyName("resolveProvider")]
    public bool? ResolveProvider { get; set; }
}
