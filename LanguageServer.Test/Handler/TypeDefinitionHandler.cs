using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeDefinition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using Range = EmmyLua.LanguageServer.Framework.Protocol.Model.Range;


namespace EmmyLua.LanguageServer.Framework.Handler;

public class TypeDefinitionHandler : TypeDefinitionHandlerBase
{
    protected override Task<TypeDefinitionResponse?> Handle(TypeDefinitionParams request,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine("TypeDefinitionHandler.Handle");
        return Task.FromResult(new TypeDefinitionResponse(new Location(request.TextDocument.Uri,
            new Range() { Start = new Position(0, 0), End = new Position(0, 1) }
        )))!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.TypeDefinitionProvider = true;
    }
}