using System.Buffers;
using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolReader(Stream inputStream, JsonSerializerOptions jsonSerializerOptions)
{
    private int _startIndex = 0;

    private byte[] SmallBuffer { get; } = new byte[1024];

    public async Task<MethodMessage> ReadAsync()
    {
        // Read the header part
        var (totalLength, contentStart, readContentLength) = await ReadHeadersAsync();

        if (totalLength + contentStart <= SmallBuffer.Length)
        {
            return await ReadSmallJsonRpcMessageAsync(totalLength, contentStart, readContentLength);
        }
        else
        {
            return await ReadLargeJsonRpcMessageAsync(totalLength, contentStart, readContentLength);
        }
    }

    // FIX me
    private async Task<(int, int, int)> ReadHeadersAsync()
    {
        var totalLength = 0;
        var contentStart = 0;
        var readContentLength = 0;

        var parseStart = 0;
        var readLength = 0;
        while (true)
        {
            var read = await inputStream.ReadAsync(SmallBuffer.AsMemory());
            if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");

            readLength += read;

            var lineEnd = Array.IndexOf(SmallBuffer, (byte)'\n', 0, readLength);
        }

        // if (headerEnd >= 0)
        // {
        //     var header = Encoding.UTF8.GetString(SmallBuffer, 0, headerEnd);
        //     if (header.StartsWith("Content-Length:"))
        //     {
        //         if (!int.TryParse(header["Content-Length:".Length..].Trim(), out totalLength))
        //         {
        //             throw new InvalidOperationException("Invalid Content-Length header.");
        //         }
        //     }
        //
        //     if (headerEnd + 1 < readContentLength)
        //     {
        //         contentStart = headerEnd + 1;
        //         break;
        //     }
        // }

        return (totalLength, contentStart, readContentLength);
    }
    private async Task<MethodMessage> ReadSmallJsonRpcMessageAsync(int totalContentLength, int contentStart, int readContentLength)
    {
        try
        {
            var bytesRead = contentStart + readContentLength;
            while (bytesRead < totalContentLength)
            {
                var bufferSpan = SmallBuffer.AsMemory(bytesRead, totalContentLength - bytesRead);
                var read = await inputStream.ReadAsync(bufferSpan);
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                bytesRead += read;
            }

            return JsonSerializer.Deserialize<MethodMessage>(SmallBuffer.AsSpan(contentStart, totalContentLength), jsonSerializerOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON-RPC message.", ex);
        }
    }

    private async Task<MethodMessage> ReadLargeJsonRpcMessageAsync(int totalContentLength, int contentStart, int readContentLength)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(totalContentLength);
        // 将smallbuffer中的数据拷贝到buffer中
        SmallBuffer.AsSpan(contentStart, readContentLength).CopyTo(buffer);
        try
        {
            var bytesRead = readContentLength;
            while (bytesRead < totalContentLength)
            {
                var bufferSpan = buffer.AsMemory(bytesRead, totalContentLength - bytesRead);
                var read = await inputStream.ReadAsync(bufferSpan);
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                bytesRead += read;
            }

            return JsonSerializer.Deserialize<MethodMessage>(buffer.AsSpan(0, totalContentLength), jsonSerializerOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON-RPC message.", ex);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
