using System.Buffers;
using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Framework.Server.JsonProtocol;

public class JsonProtocolReader(Stream inputStream, JsonSerializerOptions jsonSerializerOptions)
{
    private int _currentValidLength = 0;

    private byte[] SmallBuffer { get; } = new byte[1024];

    public async Task<MethodMessage> ReadAsync()
    {
        // Read the header part
        var (totalLength, contentStart) = await ReadOneHeaderAsync();
        var readContentLength = _currentValidLength - contentStart;
        if (readContentLength > totalLength)
        {
             readContentLength = totalLength;
        }

        try
        {
            if (totalLength + contentStart <= SmallBuffer.Length)
            {
                return await ReadSmallJsonRpcMessageAsync(totalLength, contentStart, readContentLength);
            }
            else
            {
                return await ReadLargeJsonRpcMessageAsync(totalLength, contentStart, readContentLength);
            }
        }
        finally
        {
            if (contentStart + readContentLength < _currentValidLength)
            {
                var remaining = _currentValidLength - (contentStart + readContentLength);
                Array.Copy(SmallBuffer, contentStart + readContentLength, SmallBuffer, 0, remaining);
                _currentValidLength = remaining;
            }
            else
            {
                _currentValidLength = 0;
            }
        }
    }

    private async Task ReadHeaderToBufferAsync()
    {
        var read = await inputStream.ReadAsync(SmallBuffer.AsMemory(_currentValidLength));
        if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
        _currentValidLength += read;
    }

    private bool TryGetContentLength(int startIndex, out int contentLength, out int contentStart)
    {
        contentLength = 0;
        contentStart = 0;

        for (var i = startIndex; i < _currentValidLength; i++)
        {
            if (SmallBuffer[i] == '\r' &&
                (i + 1 < _currentValidLength && SmallBuffer[i + 1] == '\n'))
            {
                var headerEnd = i;
                if (headerEnd >= 0)
                {
                    var header = Encoding.UTF8.GetString(SmallBuffer, startIndex, headerEnd - startIndex);
                    if (header.StartsWith("Content-Length:"))
                    {
                        if (!int.TryParse(header["Content-Length:".Length..].Trim(), out contentLength))
                        {
                            throw new InvalidOperationException("Invalid Content-Length header.");
                        }
                    }
                }
                startIndex = i + 2;
                if (startIndex + 1 < _currentValidLength && SmallBuffer[startIndex] == '\r' && SmallBuffer[startIndex + 1] == '\n')
                {
                    contentStart = startIndex + 2;
                    return true;
                }
            }
        }

        return false;
    }

    private async Task<(int, int)> ReadOneHeaderAsync()
    {
        var totalLength = 0;
        var contentStart = 0;

        while (true)
        {
            if (TryGetContentLength(0, out totalLength, out contentStart))
            {
                break;
            }
            await ReadHeaderToBufferAsync();
        }

        return (totalLength, contentStart);
    }

    private async Task<MethodMessage> ReadSmallJsonRpcMessageAsync(int totalContentLength, int contentStart,
        int readContentLength)
    {
        try
        {
            while (readContentLength < totalContentLength)
            {
                var read = await inputStream.ReadAsync(SmallBuffer.AsMemory(contentStart + readContentLength));
                if (read == 0) throw new InvalidOperationException("Stream closed before all data could be read.");
                readContentLength += read;
            }

            return JsonSerializer.Deserialize<MethodMessage>(SmallBuffer.AsSpan(contentStart, totalContentLength),
                jsonSerializerOptions)!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON-RPC message.", ex);
        }
    }

    private async Task<MethodMessage> ReadLargeJsonRpcMessageAsync(int totalContentLength, int contentStart,
        int readContentLength)
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

            return JsonSerializer.Deserialize<MethodMessage>(buffer.AsSpan(0, totalContentLength),
                jsonSerializerOptions)!;
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
