using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;

public class ReferenceContext
{
    /**
     * Include the declaration of the current symbol.
     */
    [JsonPropertyName("includeDeclaration")]
    public bool IncludeDeclaration { get; set; }
}
