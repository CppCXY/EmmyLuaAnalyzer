using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolWriter(Stream output, JsonSerializerOptions jsonSerializerOptions)
{
    public void WriteResponse(StringOrInt id, object? message, Type messageType)
    {
        var result = JsonSerializer.SerializeToDocument(message, messageType, jsonSerializerOptions);
        var response = new ResponseMessage(id, result, null);
        var json = JsonSerializer.Serialize(response, jsonSerializerOptions);
        var contentLength = Encoding.UTF8.GetByteCount(json);
        var writeContent = $"Content-Length:{contentLength}\r\n\r\n{json}";
        var writeContentBytes = Encoding.UTF8.GetBytes(writeContent);
        output.Write(writeContentBytes);
    }

    public void WriteNotification(NotificationMessage message, Type messageType)
    {
        var json = JsonSerializer.Serialize(message, messageType, jsonSerializerOptions);
        var contentLength = Encoding.UTF8.GetByteCount(json);
        var writeContent = $"Content-Length:{contentLength}\r\n\r\n{json}";
        var writeContentBytes = Encoding.UTF8.GetBytes(writeContent);
        output.Write(writeContentBytes);
    }
}
