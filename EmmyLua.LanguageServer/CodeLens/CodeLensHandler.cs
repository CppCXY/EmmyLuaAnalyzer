using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeLens;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.CodeLens;

// ReSharper disable once ClassNeverInstantiated.Global
public class CodeLensHandler(ServerContext context) : CodeLensHandlerBase
{
    private CodeLensBuilder Builder { get; } = new();

    protected override Task<CodeLensResponse> Handle(CodeLensParams request, CancellationToken token)
    {
        CodeLensResponse? container = null;
        var config = context.SettingManager.GetCodeLensConfig();
        if (!config.Enable)
        {
            return Task.FromResult(container)!;
        }
        var uri = request.TextDocument.Uri.UnescapeUri;
        
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                container = new CodeLensResponse(Builder.Build(semanticModel, context));
            }
        });
        
        return Task.FromResult(container)!;
    }

    protected override Task<Framework.Protocol.Message.CodeLens.CodeLens> Resolve(Framework.Protocol.Message.CodeLens.CodeLens request, CancellationToken token)
    {
        context.ReadyRead(() =>
        {
            request = Builder.Resolve(request, context);
        });
        
        return Task.FromResult(request);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CodeLensProvider = new CodeLensOptions()
        {
            ResolveProvider = true
        };
    }
}