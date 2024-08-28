using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeHierarchy;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.TypeHierarchy;

// ReSharper disable once ClassNeverInstantiated.Global
public class TypeHierarchyHandler(ServerContext context) : TypeHierarchyHandlerBase
{
    private TypeHierarchyBuilder Builder { get; } = new();

    protected override Task<TypeHierarchyResponse?> Handle(TypeHierarchyPrepareParams typeHierarchyPrepareParams,
        CancellationToken cancellationToken)
    {
        TypeHierarchyResponse? result = null;
        var uri = typeHierarchyPrepareParams.TextDocument.Uri.UnescapeUri;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var node = semanticModel.Document.SyntaxTree.SyntaxRoot.NameNodeAt(
                    typeHierarchyPrepareParams.Position.Line,
                    typeHierarchyPrepareParams.Position.Character);
                if (node is not null)
                {
                    var list = Builder.BuildPrepare(semanticModel, node);
                    if (list is not null)
                    {
                        result = new TypeHierarchyResponse(list);
                    }
                }
            }
        });

        return Task.FromResult(result);
    }

    protected override Task<TypeHierarchyResponse?> Handle(TypeHierarchySupertypesParams typeHierarchySupertypesParams,
        CancellationToken cancellationToken)
    {
        TypeHierarchyResponse? result = null;
        context.ReadyRead(() =>
        {
            if (typeHierarchySupertypesParams.Item.Data?.Value is string str)
            {
                var parts = str.Split('|');
                if (parts.Length != 2)
                {
                    return;
                }

                if (int.TryParse(parts[0], out var id))
                {
                    var namedType = new LuaNamedType(new(id), parts[1]);
                    result = new TypeHierarchyResponse(Builder.BuildSupers(context.LuaProject.Compilation, namedType));
                }
            }
        });

        return Task.FromResult(result);
    }

    protected override Task<TypeHierarchyResponse?> Handle(TypeHierarchySubtypesParams typeHierarchySubtypesParams,
        CancellationToken cancellationToken)
    {
        TypeHierarchyResponse? result = null;
        context.ReadyRead(() =>
        {
            if (typeHierarchySubtypesParams.Item.Data?.Value is string str)
            {
                var parts = str.Split('|');
                if (parts.Length != 2)
                {
                    return;
                }

                if (int.TryParse(parts[0], out var id))
                {
                    var namedType = new LuaNamedType(new(id), parts[1]);
                    result = new(Builder.BuildSubTypes(context.LuaProject.Compilation, namedType));
                }
            }
        });

        return Task.FromResult(result);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.TypeHierarchyProvider = true;
    }
}