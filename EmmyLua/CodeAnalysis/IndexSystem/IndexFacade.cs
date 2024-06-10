using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.IndexSystem;

public class IndexFacade : IQueryableIndex
{
    private LuaCompilation Compilation { get; }

    public WorkspaceIndex WorkspaceIndex { get; } = new();

    private List<IQueryableIndex> QueryableIndexes { get; } = new();

    public IndexFacade(LuaCompilation compilation)
    {
        Compilation = compilation;
        QueryableIndexes.Add(WorkspaceIndex);
    }

    public void AddQueryableIndex(IQueryableIndex queryableIndex)
    {
        if (!ReferenceEquals(this, queryableIndex))
        {
            QueryableIndexes.Add(queryableIndex);
        }
    }

    public void RemoveQueryableIndex(IQueryableIndex queryableIndex)
    {
        QueryableIndexes.Remove(queryableIndex);
    }

    public IEnumerable<IDeclaration> QueryMembers(string name)
    {
        if (name == "global")
        {
            return QueryAllGlobal();
        }

        return QueryableIndexes.SelectMany(it => it.QueryMembers(name));
    }

    public IEnumerable<LuaDeclaration> QueryAllMembers()
    {
        // do not query extension
        return WorkspaceIndex.TypeMembers.QueryAll();
    }

    public IEnumerable<IDeclaration> QueryGlobals(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryGlobals(name));
    }

    public IEnumerable<IDeclaration> QueryAllGlobal()
    {
        return QueryableIndexes.SelectMany(it => it.QueryAllGlobal());
    }

    public IEnumerable<LuaType> QuerySupers(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QuerySupers(name));
    }

    public IEnumerable<string> QuerySubTypes(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QuerySubTypes(name));
    }

    public IEnumerable<IDeclaration> QueryNamedTypeDefinitions(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryNamedTypeDefinitions(name));
    }

    public LuaType? QueryTypeFromId(SyntaxElementId id)
    {
        return WorkspaceIndex.IdRelatedType.Query(id.DocumentId, id);
    }

    public IEnumerable<LuaType> QueryAliasOriginTypes(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryAliasOriginTypes(name));
    }

    public LuaType? QueryModuleType(LuaDocumentId documentId)
    {
        return WorkspaceIndex.ModuleTypes.GetValueOrDefault(documentId);
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> QueryModuleReturns(LuaDocumentId documentId)
    {
        return WorkspaceIndex.ModuleReturns.GetValueOrDefault(documentId) ?? [];
    }

    public IEnumerable<IDeclaration> QueryGenericParams(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryGenericParams(name));
    }

    public NamedTypeKind QueryNamedTypeKind(string name)
    {
        foreach (var index in QueryableIndexes)
        {
            var kind = index.QueryNamedTypeKind(name);
            if (kind != NamedTypeKind.None)
            {
                return kind;
            }
        }

        return NamedTypeKind.None;
    }

    public IEnumerable<LuaNameExprSyntax> QueryNameExprReferences(string name)
    {
        foreach (var ptr in WorkspaceIndex.NameExpr.Query(name))
        {
            var node = ptr.ToNode(Compilation.Workspace);
            if (node is not null)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaIndexExprSyntax> QueryIndexExprReferences(string name)
    {
        foreach (var ptr in WorkspaceIndex.IndexExpr.Query(name))
        {
            var node = ptr.ToNode(Compilation.Workspace);
            if (node is not null)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaDocNameTypeSyntax> QueryNamedTypeReferences(string name)
    {
        foreach (var nameTypePtr in WorkspaceIndex.NameType.Query(name))
        {
            if (nameTypePtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public LuaNamedType? QueryParentType(LuaSyntaxNode node)
    {
        var parentType = WorkspaceIndex.ParentType.Query(node.UniqueId.DocumentId, node.UniqueId);
        if (parentType is not null)
        {
            return new LuaNamedType(parentType);
        }

        return null;
    }

    public LuaNamedType? QueryParentType(IDeclaration declaration)
    {
        if (declaration is LuaDeclaration luaDeclaration)
        {
            var parentType = WorkspaceIndex.ParentType.Query(luaDeclaration.UniqueId.DocumentId, luaDeclaration.UniqueId);
            if (parentType is not null)
            {
                return new LuaNamedType(parentType);
            }
        }

        return null;
    }

    public IEnumerable<IDeclaration> QueryAllNamedTypeDefinitions()
    {
        return QueryableIndexes.SelectMany(it => it.QueryAllNamedTypeDefinitions());
    }

    public IEnumerable<TypeOperator> QueryTypeOperators(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryTypeOperators(name));
    }

    public IEnumerable<LuaMethodType> QueryTypeOverloads(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryTypeOverloads(name));
    }

    public IEnumerable<LuaReference> QueryLocalReferences(LuaDeclaration luaDeclaration)
    {
        return WorkspaceIndex.QueryLocalReferences(luaDeclaration);
    }

    public LuaDeclaration? QueryLocalDeclaration(LuaSyntaxElement element)
    {
        return WorkspaceIndex.QueryLocalDeclaration(element);
    }

    public IEnumerable<LuaDeclaration> QueryDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        return WorkspaceIndex.QueryDocumentLocalDeclarations(documentId);
    }

    public LuaDeclarationTree? QueryDeclarationTree(LuaDocumentId documentId)
    {
        return WorkspaceIndex.QueryDeclarationTree(documentId);
    }

    public void Remove(LuaDocumentId documentId)
    {
        WorkspaceIndex.Remove(documentId);
    }
}
