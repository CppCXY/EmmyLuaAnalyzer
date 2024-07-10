using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;
using EmmyLua.LanguageServer.Framework.Server.Handler;

namespace EmmyLua.LanguageServer.Framework.Handler;

public class CallHierarchyHandler : CallHierarchyHandlerBase
{
    protected override Task<CallHierarchyPrepareResponse> CallHierarchyPrepare(CallHierarchyPrepareParams request, CancellationToken token)
    {
        Console.Error.WriteLine("CallHierarchyPrepare");
        return Task.FromResult(new CallHierarchyPrepareResponse(new List<CallHierarchyItem>()));
    }

    protected override Task<CallHierarchyIncomingCallsResponse> CallHierarchyIncomingCalls(CallHierarchyIncomingCallsParams request, CancellationToken token)
    {
        Console.Error.WriteLine("CallHierarchyIncomingCalls");
        return Task.FromResult(new CallHierarchyIncomingCallsResponse(new List<CallHierarchyIncomingCall>()));
    }

    protected override Task<CallHierarchyOutgoingCallsResponse> CallHierarchyOutgoingCalls(CallHierarchyOutgoingCallsParams request, CancellationToken token)
    {
        Console.Error.WriteLine("CallHierarchyOutgoingCalls");
        return Task.FromResult(new CallHierarchyOutgoingCallsResponse(new List<CallHierarchyOutgoingCall>()));
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CallHierarchyProvider = true;
    }
}