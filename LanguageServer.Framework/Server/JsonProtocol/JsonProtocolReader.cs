using System.Buffers;
using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolReader(Stream inputStream, JsonSerializerOptions jsonSerializerOptions)
{
    private StreamReader Reader { get; } = new(inputStream, Encoding.UTF8);
    
    public async Task<Message> ReadAsync()
    {
        // Read the header part
        var headers = await ReadHeadersAsync();
        if (!headers.TryGetValue("Content-Length", out var contentLengthStr) ||
            !int.TryParse(contentLengthStr, out var contentLength))
        {
            throw new InvalidOperationException("Invalid LSP header: Content-Length is missing or invalid.");
        }

        // Read the JSON-RPC message part
        return await ReadJsonRpcMessageAsync(contentLength);
    }

    private async Task<Dictionary<string, string>> ReadHeadersAsync()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        while (await Reader.ReadLineAsync() is { } line)
        {
            if (line == "") break; // Empty line indicates end of headers

            var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                headers[parts[0]] = parts[1];
            }
        }

        return headers;
    }

    private async Task<Message> ReadJsonRpcMessageAsync(int contentLength)
    {
        var buffer = ArrayPool<char>.Shared.Rent(contentLength);
        try
        {
            var bytesRead = 0;
            while (bytesRead < contentLength)
            {
                var bufferSpan = buffer.AsMemory(bytesRead, contentLength - bytesRead);
                var read = await Reader.ReadAsync(bufferSpan);
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                bytesRead += read;
            }

            return JsonSerializer.Deserialize<MethodMessage>(buffer.AsSpan(0, contentLength), jsonSerializerOptions)!;
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}