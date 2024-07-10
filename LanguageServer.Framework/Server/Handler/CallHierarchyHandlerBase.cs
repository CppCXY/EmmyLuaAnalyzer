using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class CallHierarchyHandlerBase : IJsonHandler
{
    protected abstract Task<CallHierarchyPrepareResponse> CallHierarchyPrepare(CallHierarchyPrepareParams request,
        CancellationToken token);

    protected abstract Task<CallHierarchyIncomingCallsResponse> CallHierarchyIncomingCalls(
        CallHierarchyIncomingCallsParams request, CancellationToken token);

    protected abstract Task<CallHierarchyOutgoingCallsResponse> CallHierarchyOutgoingCalls(
        CallHierarchyOutgoingCallsParams request, CancellationToken token);

    public void RegisterHandler(LanguageServer server)
    {
        server.AddRequestHandler("textDocument/prepareCallHierarchy", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CallHierarchyPrepareParams>(server.JsonSerializerOptions)!;
            var r = await CallHierarchyPrepare(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });

        server.AddRequestHandler("callHierarchy/incomingCalls", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CallHierarchyIncomingCallsParams>(server.JsonSerializerOptions)!;
            var r = await CallHierarchyIncomingCalls(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });

        server.AddRequestHandler("callHierarchy/outgoingCalls", async (message, token) =>
        {
            var request = message.Params!.Deserialize<CallHierarchyOutgoingCallsParams>(server.JsonSerializerOptions)!;
            var r = await CallHierarchyOutgoingCalls(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);
}
