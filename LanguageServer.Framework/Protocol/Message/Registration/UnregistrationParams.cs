using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Registration;

public class UnregistrationParams
{
    // This should correctly be named `unregistrations`. However, changing this
    // is a breaking change and needs to wait until we deliver a 4.x version
    // of the specification.
    [JsonPropertyName("unregisterations")]
    public List<Unregistration> Unregisterations { get; set; } = null!;
}
