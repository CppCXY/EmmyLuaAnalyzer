using LanguageServer.Server;
using LanguageServer.Util;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.TypeHierarchy;

// ReSharper disable once ClassNeverInstantiated.Global
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
                var node = semanticModel.Document.SyntaxTree.SyntaxRoot.NameNodeAt(request.Position.Line,
                    request.Position.Character);
                if (node is not null)
                {
                    result = Builder.BuildPrepare(semanticModel, node);
                }
            }
        });

        return Task.FromResult(result);
    }

    public override Task<Container<TypeHierarchyItem>?> Handle(TypeHierarchySupertypesParams request,
        CancellationToken cancellationToken)
    {
        Container<TypeHierarchyItem>? result = null;
        context.ReadyRead(() =>
        {
            if (request.Item.Data?.Type == JTokenType.String && request.Item.Data?.Value<string>() is { } name)
            {
                result = Builder.BuildSupers(context.LuaWorkspace.Compilation, name);
            }
        });

        return Task.FromResult(result);
    }

    public override Task<Container<TypeHierarchyItem>?> Handle(TypeHierarchySubtypesParams request,
        CancellationToken cancellationToken)
    {
        Container<TypeHierarchyItem>? result = null;
        context.ReadyRead(() =>
        {
            if (request.Item.Data?.Type == JTokenType.String && request.Item.Data?.Value<string>() is { } name)
            {
                result = Builder.BuildSubTypes(context.LuaWorkspace.Compilation, name);
            }
        });

        return Task.FromResult(result);
    }
}