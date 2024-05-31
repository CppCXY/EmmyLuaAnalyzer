using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class DbManager : QueryableIndex
{
    private LuaCompilation Compilation { get; }

    private WorkspaceIndexes WorkspaceIndexes { get; } = new();

    private List<QueryableIndex> QueryableIndexes { get; } = new();

    public DbManager(LuaCompilation compilation)
    {
        Compilation = compilation;
        QueryableIndexes.Add(WorkspaceIndexes);
    }

    public void AddQueryableIndex(QueryableIndex queryableIndex)
    {
        QueryableIndexes.Add(queryableIndex);
    }

    public void RemoveQueryableIndex(QueryableIndex queryableIndex)
    {
        QueryableIndexes.Remove(queryableIndex);
    }

    private static HashSet<string> NotMemberNames { get; } =
    [
        "unknown",
        "nil",
        "boolean",
        "number",
        "int",
        "integer",
        "function",
        "thread",
        "userdata",
        "any",
        "void",
        "never",
        "self",
        "T"
    ];

    public void AddMember(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        if (NotMemberNames.Contains(name))
        {
            return;
        }

        if (name == "global")
        {
            AddGlobal(documentId, luaDeclaration.Name, luaDeclaration);
            return;
        }

        WorkspaceIndexes.TypeMembers.Add(documentId, name, luaDeclaration);
        WorkspaceIndexes.ParentType.Add(documentId, luaDeclaration.Info.Ptr.UniqueId, name);
    }

    public void AddGlobal(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        WorkspaceIndexes.Globals.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType type)
    {
        WorkspaceIndexes.Supers.Add(documentId, name, type);
        if (type is LuaNamedType namedType)
        {
            WorkspaceIndexes.SubTypes.Add(documentId, namedType.Name, name);
        }
    }

    public void AddTypeDefinition(LuaDocumentId documentId, string name, LuaDeclaration declaration)
    {
        WorkspaceIndexes.NamedTypeDefinition.Add(documentId, name, declaration);
    }

    public void AddAlias(LuaDocumentId documentId, string name, LuaType baseType,
        LuaDeclaration declaration)
    {
        AddTypeDefinition(documentId, name, declaration);
        WorkspaceIndexes.AliasTypes.Add(documentId, name, baseType);
    }

    public void AddIdRelatedType(LuaDocumentId documentId, long id, LuaType relatedType)
    {
        WorkspaceIndexes.IdRelatedType.Add(documentId, id, relatedType);
    }

    public void AddEnum(LuaDocumentId documentId, string name, LuaType? baseType,
        LuaDeclaration declaration)
    {
        AddTypeDefinition(documentId, name, declaration);
        if (baseType != null)
        {
            AddSuper(documentId, name, baseType);
        }
    }

    public void AddGenericParam(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        WorkspaceIndexes.GenericParams.Add(documentId, name, luaDeclaration);
    }

    public void AddModuleReturns(LuaDocumentId documentId, LuaType type, List<LuaExprSyntax> exprs)
    {
        WorkspaceIndexes.ModuleTypes[documentId] = type;
        WorkspaceIndexes.ModuleReturns[documentId] = exprs.Select(it => it.ToPtr<LuaExprSyntax>()).ToList();
    }

    public void AddNameExpr(LuaDocumentId documentId, LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { RepresentText: { } name })
        {
            WorkspaceIndexes.NameExpr.Add(documentId, name, new(nameExpr));
        }
    }

    public void AddIndexExpr(LuaDocumentId documentId, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr is { Name: { } name })
        {
            WorkspaceIndexes.IndexExpr.Add(documentId, name, new(indexExpr));
        }
    }

    public void AddNameType(LuaDocumentId documentId, LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            WorkspaceIndexes.NameType.Add(documentId, name, new(nameType));
        }
    }

    public void AddTypeOperator(LuaDocumentId documentId, TypeOperator typeOperator)
    {
        WorkspaceIndexes.TypeOperator.AddTypeOperator(documentId, typeOperator);
    }

    public void AddTypeOverload(LuaDocumentId documentId, string name, LuaMethodType methodType)
    {
        WorkspaceIndexes.TypeOverloads.Add(documentId, name, methodType);
    }

    public IEnumerable<LuaDeclaration> QueryMembers(string name)
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
        return WorkspaceIndexes.TypeMembers.QueryAll();
    }

    public IEnumerable<LuaDeclaration> QueryGlobals(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryGlobals(name));
    }

    public IEnumerable<LuaDeclaration> QueryAllGlobal()
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

    public IEnumerable<LuaDeclaration> QueryNamedTypeDefinitions(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryNamedTypeDefinitions(name));
    }

    public IEnumerable<LuaType> QueryTypeFromId(long id)
    {
        return WorkspaceIndexes.IdRelatedType.Query(id);
    }

    public IEnumerable<LuaType> QueryAliasOriginTypes(string name)
    {
        return QueryableIndexes.SelectMany(it => it.QueryAliasOriginTypes(name));
    }

    public LuaType? QueryModuleType(LuaDocumentId documentId)
    {
        return WorkspaceIndexes.ModuleTypes.GetValueOrDefault(documentId);
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> QueryModuleReturns(LuaDocumentId documentId)
    {
        return WorkspaceIndexes.ModuleReturns.GetValueOrDefault(documentId) ?? [];
    }

    public IEnumerable<LuaDeclaration> QueryGenericParams(string name)
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
        foreach (var ptr in  WorkspaceIndexes.NameExpr.Query(name))
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
        foreach (var ptr in WorkspaceIndexes.IndexExpr.Query(name))
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
        foreach (var nameTypePtr in WorkspaceIndexes.NameType.Query(name))
        {
            if (nameTypePtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public bool IsDefinedType(string name)
    {
        return QueryNamedTypeDefinitions(name).FirstOrDefault() is not null;
    }

    public LuaNamedType? QueryParentType(LuaSyntaxNode node)
    {
        var parentType = WorkspaceIndexes.ParentType.Query(node.UniqueId).FirstOrDefault();
        if (parentType is not null)
        {
            return new LuaNamedType(parentType);
        }

        return null;
    }

    public IEnumerable<LuaDeclaration> QueryAllNamedTypeDefinitions()
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

    public void Remove(LuaDocumentId documentId)
    {
        WorkspaceIndexes.Remove(documentId);
    }
}
