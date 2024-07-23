using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.Definition;

// ReSharper disable once ClassNeverInstantiated.Global
public class DefinitionHandler(ServerContext context) : DefinitionHandlerBase
{
    protected override Task<DefinitionResponse?> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        DefinitionResponse? locationLinks = null;
        context.ReadyRead(() =>
        {
            var workspace = context.LuaProject;
            var semanticModel = workspace.Compilation.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var document = semanticModel.Document;
                var pos = request.Position;
                var token = document.SyntaxTree.SyntaxRoot.TokenAt(pos.Line, pos.Character);
                if (token is LuaStringToken module
                    && token.Parent?.Parent?.Parent is LuaCallExprSyntax { Name: { } funcName }
                    && workspace.Features.RequireLikeFunction.Contains(funcName))
                {
                    var moduleDocument = workspace.ModuleManager.FindModule(module.Value);
                    if (moduleDocument is not null)
                    {
                        locationLinks = new DefinitionResponse(
                            moduleDocument.SyntaxTree.SyntaxRoot.Location.ToLspLocation()
                        );
                        return;
                    }
                }

                var node = document.SyntaxTree.SyntaxRoot.NameNodeAt(pos.Line, pos.Character);
                if (node is not null)
                {
                    var declaration = semanticModel.Context.FindDeclaration(node);
                    if (declaration?.GetLocation(semanticModel.Context) is { } location)
                    {
                        locationLinks = new DefinitionResponse(
                            location.ToLspLocation()
                        );
                    }
                }
            }
        });

        return Task.FromResult(locationLinks);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.DefinitionProvider = true;
    }
}