using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.TypeHierarchy;

public class TypeHierarchyHandler(ServerContext context) : TypeHierarchyHandlerBase
{
    private TypeHierarchyBuilder Builder { get; } = new();

    protected override TypeHierarchyRegistrationOptions CreateRegistrationOptions(TypeHierarchyCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace)
        };
    }

    public override Task<Container<TypeHierarchyItem>?> Handle(TypeHierarchyPrepareParams request,
        CancellationToken cancellationToken)
    {
        Container<TypeHierarchyItem>? result = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(request.TextDocument.Uri.ToUnencodedString());
            if (semanticModel is not null)
            {
                var node = semanticModel.Document.SyntaxTree.SyntaxRoot.NodeAt(request.Position.Line,
                    request.Position.Character);
                result = Builder.BuildPrepare(semanticModel, node);
            }
        });

        return Task.FromResult(result);
    }

    public override Task<Container<TypeHierarchyItem>?> Handle(TypeHierarchySupertypesParams request,
        CancellationToken cancellationToken)
    {
        var result = new Container<TypeHierarchyItem>(request.Item);
        return Task.FromResult(result)!;
    }

    public override Task<Container<TypeHierarchyItem>?> Handle(TypeHierarchySubtypesParams request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<Container<TypeHierarchyItem>?>(null);
    }
}