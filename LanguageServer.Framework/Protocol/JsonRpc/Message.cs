using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

public record Message(string JsonRpc)
{
    [JsonPropertyName("jsonrpc")] public string JsonRpc { get; } = JsonRpc;
}

[JsonConverter(typeof(MethodMessageConverter))]
public record MethodMessage(
    string Method
) : Message("2.0")
{
    [JsonPropertyName("method")] public string Method { get; } = Method;
}

public record RequestMessage(
    StringOrInt Id,
    string Method,
    JsonDocument? Params
) : MethodMessage(Method)
{
    [JsonPropertyName("id")]
    public StringOrInt Id { get; } = Id;

    [JsonPropertyName("params")] public JsonDocument? Params { get; } = Params;
}

public static class ErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
    public const int ServerErrorStart = -32099;
    public const int ServerErrorEnd = -32000;
    public const int ServerNotInitialized = -32002;
    public const int UnknownErrorCode = -32001;
    public const int JsonrpcReservedErrorRangeEnd = -32000;
    public const int LspReservedErrorRangeStart = -32899;
    public const int RequestFailed = -32803;
    public const int ServerCancelled = -32802;
    public const int ContentModified = -32801;
    public const int RequestCancelled = -32800;
    public const int LspReservedErrorRangeEnd = -32800;
}

public record ResponseError(
    int Code,
    string Message,
    object? Data
)
{
    [JsonPropertyName("code")] public int Code { get; } = Code;

    [JsonPropertyName("message")] public string Message { get; } = Message;

    [JsonPropertyName("data")] public object? Data { get; } = Data;
}

public record ResponseMessage(
    StringOrInt Id,
    JsonDocument? Result,
    ResponseError? Error
) : Message("2.0")
{
    [JsonPropertyName("id")]
    public StringOrInt Id { get; } = Id;

    [JsonPropertyName("result")] public JsonDocument? Result { get; } = Result;

    [JsonPropertyName("error")] public ResponseError? Error { get; } = Error;
}

public record NotificationMessage(
    string Method,
    JsonDocument? Params
) : MethodMessage(Method)
{
    [JsonPropertyName("params")] public JsonDocument? Params { get; } = Params;
}
