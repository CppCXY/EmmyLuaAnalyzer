using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class WorkspaceIndexes: QueryableIndex
{
    public IndexStorage<string, LuaDeclaration> TypeMembers { get; } = new();

    public IndexStorage<long, string> ParentType { get; } = new();

    public IndexStorage<string, LuaDeclaration> Globals { get; } = new();

    public IndexStorage<string, LuaType> Supers { get; } = new();

    public IndexStorage<string, string> SubTypes { get; } = new();

    public IndexStorage<string, LuaDeclaration> NamedTypeDefinition { get; } = new();

    public IndexStorage<long, LuaType> IdRelatedType { get; } = new();

    public IndexStorage<string, LuaType> AliasTypes { get; } = new();

    public IndexStorage<string, LuaDeclaration> GenericParams { get; } = new();

    public Dictionary<LuaDocumentId, LuaType> ModuleTypes { get; } = new();

    public Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ModuleReturns { get; } = new();

    public TypeOperatorStorage TypeOperator { get; } = new();

    public IndexStorage<string, LuaElementPtr<LuaNameExprSyntax>> NameExpr { get; } = new();

    public IndexStorage<string, LuaElementPtr<LuaIndexExprSyntax>> IndexExpr { get; } = new();

    public IndexStorage<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameType { get; } = new();

    public IndexStorage<string, LuaMethodType> TypeOverloads { get; } = new();

    public void Remove(LuaDocumentId documentId)
    {
        TypeMembers.Remove(documentId);
        ParentType.Remove(documentId);
        Globals.Remove(documentId);
        Supers.Remove(documentId);
        SubTypes.Remove(documentId);
        NamedTypeDefinition.Remove(documentId);
        IdRelatedType.Remove(documentId);
        GenericParams.Remove(documentId);
        ModuleTypes.Remove(documentId);
        TypeOperator.Remove(documentId);
        NameExpr.Remove(documentId);
        IndexExpr.Remove(documentId);
        NameType.Remove(documentId);
        AliasTypes.Remove(documentId);
        TypeOverloads.Remove(documentId);
    }

    public IEnumerable<LuaDeclaration> QueryAllGlobal()
    {
        return Globals.QueryAll();
    }

    public IEnumerable<LuaDeclaration> QueryMembers(string name)
    {
        return TypeMembers.Query(name);
    }

    public IEnumerable<LuaDeclaration> QueryGlobals(string name)
    {
        return Globals.Query(name);
    }

    public IEnumerable<LuaType> QuerySupers(string name)
    {
        return Supers.Query(name);
    }

    public IEnumerable<string> QuerySubTypes(string name)
    {
        return SubTypes.Query(name);
    }

    public IEnumerable<LuaDeclaration> QueryNamedTypeDefinitions(string name)
    {
        return NamedTypeDefinition.Query(name);
    }

    public IEnumerable<LuaType> QueryAliasOriginTypes(string name)
    {
        return AliasTypes.Query(name);
    }

    public IEnumerable<LuaDeclaration> QueryGenericParams(string name)
    {
        return GenericParams.Query(name);
    }

    public NamedTypeKind QueryNamedTypeKind(string name)
    {
        var typeDeclaration = QueryNamedTypeDefinitions(name).FirstOrDefault();
        if (typeDeclaration is { Info: NamedTypeInfo { Kind: { } kind } })
        {
            return kind;
        }

        return NamedTypeKind.None;
    }

    public IEnumerable<LuaDeclaration> QueryAllNamedTypeDefinitions()
    {
        return NamedTypeDefinition.QueryAll();
    }

    public IEnumerable<TypeOperator> QueryTypeOperators(string name)
    {
        return TypeOperator.GetTypeOperators(name);
    }

    public IEnumerable<LuaMethodType> QueryTypeOverloads(string name)
    {
        return TypeOverloads.Query(name);
    }
}
