using System.Buffers;
using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.Reader;

public class JsonProtocolReader(Stream inputStream)
{
    private StreamReader Reader { get; } = new(inputStream, Encoding.UTF8);
    
    public async Task<Message> ReadAsync()
    {
        // Read the header part
        var headers = await ReadHeadersAsync();
        if (!headers.TryGetValue("Content-Length", out var contentLengthStr) || !int.TryParse(contentLengthStr, out var contentLength))
        {
            throw new InvalidOperationException("Invalid LSP header: Content-Length is missing or invalid.");
        }

        // Read the JSON-RPC message part
        var jsonRpcMessage = await ReadJsonRpcMessageAsync(contentLength);

        // Deserialize the JSON-RPC message
        return JsonSerializer.Deserialize<Message>(jsonRpcMessage)!;
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

    private async Task<string> ReadJsonRpcMessageAsync(int contentLength)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(contentLength);
        try
        {
            var bytesRead = 0;
            while (bytesRead < contentLength)
            {
                var read = await inputStream.ReadAsync(buffer.AsMemory(bytesRead, contentLength - bytesRead));
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                bytesRead += read;
            }

            return Encoding.UTF8.GetString(buffer, 0, contentLength);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);   
        }
    }
}