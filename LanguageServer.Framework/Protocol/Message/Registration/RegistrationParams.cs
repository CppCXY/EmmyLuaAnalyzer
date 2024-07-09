using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Registration;

public class RegistrationParams
{
    [JsonPropertyName("registrations")]
    public List<Registration> Registrations { get; set; } = null!;
}
