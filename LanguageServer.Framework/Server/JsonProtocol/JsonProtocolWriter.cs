using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolWriter(Stream output, JsonSerializerOptions jsonSerializerOptions)
{
    public void Write(object? message, Type messageType)
    {
        var json = JsonSerializer.Serialize(message, messageType, jsonSerializerOptions);
        var contentLength = Encoding.UTF8.GetByteCount(json);
        var writeContent = $"Content-Length:{contentLength}\r\n\r\n{json}";
        var writeContentBytes = Encoding.UTF8.GetBytes(writeContent);
        output.Write(writeContentBytes);
    }
}
