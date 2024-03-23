using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.ExtensionUtil;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Definition;

// ReSharper disable once ClassNeverInstantiated.Global
public class DefinitionHandler(LuaWorkspace workspace) : DefinitionHandlerBase
{
    protected override DefinitionRegistrationOptions CreateRegistrationOptions(DefinitionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new DefinitionRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector
            (
                new TextDocumentFilter()
                {
                    Pattern = "**/*.lua"
                }
            )
        };
    }

    public override Task<LocationOrLocationLinks?> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
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
                var moduleDocument = workspace.ModuleGraph.FindModule(module.Value);
                if (moduleDocument is not null)
                {
                    return Task.FromResult<LocationOrLocationLinks?>(LocationOrLocationLinks.From(
                        moduleDocument.SyntaxTree.SyntaxRoot.Location.ToLspLocation()
                    ));
                }
            }
            
            var node = document.SyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
            var declarationTree = semanticModel.DeclarationTree;
            if (node is not null)
            {
                var declaration = declarationTree.FindDeclaration(node, semanticModel.Context);
                if (declaration?.SyntaxElement is { Location: {} location })
                {
                    return Task.FromResult<LocationOrLocationLinks?>(LocationOrLocationLinks.From(
                        location.ToLspLocation()
                        ));
                }
            }
        }

        return Task.FromResult<LocationOrLocationLinks?>(null);
    }
}