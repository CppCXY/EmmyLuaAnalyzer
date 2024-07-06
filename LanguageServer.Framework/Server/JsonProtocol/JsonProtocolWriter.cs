using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolWriter(Stream output, JsonSerializerOptions jsonSerializerOptions)
{
    private StreamWriter Writer { get; } = new(output, Encoding.UTF8);
    
    public async Task WriteAsync(object? message, Type messageType)
    {
        var json = JsonSerializer.Serialize(message, messageType, jsonSerializerOptions);
        var contentLength = Encoding.UTF8.GetByteCount(json);
        await Writer.WriteLineAsync($"Content-Length: {contentLength}\r\n");
        await Writer.WriteAsync(json);
        await Writer.FlushAsync();
    }
}