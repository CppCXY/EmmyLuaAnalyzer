using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

public interface IMessage
{
    public string JsonRpc { get; set; }
}