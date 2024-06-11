using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class WorkspaceIndex : IQueryableIndex
{
    public IndexStorage<string, LuaDeclaration> TypeMembers { get; } = new();

    public InFiledDictionary<SyntaxElementId, string> ParentType { get; } = new();

    private IndexStorage<string, LuaDeclaration> Globals { get; } = new();

    private IndexStorage<string, LuaType> Supers { get; } = new();

    private IndexStorage<string, string> SubTypes { get; } = new();

    private IndexStorage<string, LuaDeclaration> NamedTypeDefinition { get; } = new();

    public InFiledDictionary<SyntaxElementId, LuaType> IdRelatedType { get; } = new();

    private IndexStorage<string, LuaType> AliasTypes { get; } = new();

    private IndexStorage<string, LuaDeclaration> GenericParams { get; } = new();

    public Dictionary<LuaDocumentId, LuaType> ModuleTypes { get; } = new();

    public Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ModuleReturns { get; } = new();

    private IndexStorage<string, TypeOperator> TypeOperator { get; } = new();

    public IndexStorage<string, LuaElementPtr<LuaNameExprSyntax>> NameExpr { get; } = new();

    public IndexStorage<string, LuaElementPtr<LuaIndexExprSyntax>> IndexExpr { get; } = new();

    public IndexStorage<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameType { get; } = new();

    private IndexStorage<string, LuaMethodType> TypeOverloads { get; } = new();

    private InFiledDictionary<SyntaxElementId, List<LuaReference>> InFiledReferences { get; } = new();

    private InFiledDictionary<SyntaxElementId, LuaDeclaration> InFiledDeclarations { get; } = new();

    private Dictionary<LuaDocumentId, LuaDeclarationTree> DocumentDeclarationTrees { get; } = new();

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
        InFiledReferences.Remove(documentId);
        InFiledDeclarations.Remove(documentId);
        DocumentDeclarationTrees.Remove(documentId);
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

        TypeMembers.Add(documentId, name, luaDeclaration);
        ParentType.Add(documentId, luaDeclaration.Info.Ptr.UniqueId, name);
    }

    public void AddGlobal(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        Globals.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType type)
    {
        if (type is LuaNamedType { Name: { } name1 } && !string.Equals(name, name1, StringComparison.CurrentCulture))
        {
            return;
        }

        Supers.Add(documentId, name, type);
        if (type is LuaNamedType namedType)
        {
            SubTypes.Add(documentId, namedType.Name, name);
        }
    }

    public void AddTypeDefinition(LuaDocumentId documentId, string name, LuaDeclaration declaration)
    {
        NamedTypeDefinition.Add(documentId, name, declaration);
    }

    public void AddAlias(LuaDocumentId documentId, string name, LuaType baseType,
        LuaDeclaration declaration)
    {
        AddTypeDefinition(documentId, name, declaration);
        AliasTypes.Add(documentId, name, baseType);
    }

    public void AddIdRelatedType(SyntaxElementId id, LuaType relatedType)
    {
        IdRelatedType.Add(id.DocumentId, id, relatedType);
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
        GenericParams.Add(documentId, name, luaDeclaration);
    }

    public void AddModuleReturns(LuaDocumentId documentId, LuaType type, List<LuaExprSyntax> exprs)
    {
        ModuleTypes[documentId] = type;
        ModuleReturns[documentId] = exprs.Select(it => it.ToPtr<LuaExprSyntax>()).ToList();
    }

    public void AddNameExpr(LuaDocumentId documentId, LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { RepresentText: { } name })
        {
            NameExpr.Add(documentId, name, new(nameExpr));
        }
    }

    public void AddIndexExpr(LuaDocumentId documentId, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr is { Name: { } name })
        {
            IndexExpr.Add(documentId, name, new(indexExpr));
        }
    }

    public void AddNameType(LuaDocumentId documentId, LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            NameType.Add(documentId, name, new(nameType));
        }
    }

    public void AddTypeOperator(LuaDocumentId documentId, TypeOperator typeOperator)
    {
        TypeOperator.Add(documentId, typeOperator.BelongTypeName, typeOperator);
    }

    public void AddTypeOverload(LuaDocumentId documentId, string name, LuaMethodType methodType)
    {
        TypeOverloads.Add(documentId, name, methodType);
    }

    public void AddReference(LuaDocumentId documentId, LuaDeclaration declaration, LuaReference reference)
    {
        var list = InFiledReferences.Query(documentId, declaration.UniqueId);
        if (list is null)
        {
            list = [reference];
            InFiledReferences.Add(documentId, declaration.UniqueId, list);
        }
        else
        {
            list.Add(reference);
        }

        InFiledDeclarations.Add(documentId, reference.Ptr.UniqueId, declaration);
    }

    public void AddDeclarationTree(LuaDocumentId documentId, LuaDeclarationTree declarationTree)
    {
        DocumentDeclarationTrees[documentId] = declarationTree;
    }

    public IEnumerable<IDeclaration> QueryAllGlobal()
    {
        return Globals.QueryAll();
    }

    public IEnumerable<IDeclaration> QueryMembers(string name)
    {
        return TypeMembers.Query(name);
    }

    public IEnumerable<IDeclaration> QueryGlobals(string name)
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

    public IEnumerable<IDeclaration> QueryNamedTypeDefinitions(string name)
    {
        return NamedTypeDefinition.Query(name);
    }

    public IEnumerable<LuaType> QueryAliasOriginTypes(string name)
    {
        return AliasTypes.Query(name);
    }

    public IEnumerable<IDeclaration> QueryGenericParams(string name)
    {
        return GenericParams.Query(name);
    }

    public NamedTypeKind QueryNamedTypeKind(string name)
    {
        var typeDeclaration = QueryNamedTypeDefinitions(name).OfType<LuaDeclaration>().FirstOrDefault();
        if (typeDeclaration is { Info: NamedTypeInfo { Kind: { } kind } })
        {
            return kind;
        }

        return NamedTypeKind.None;
    }

    public IEnumerable<IDeclaration> QueryAllNamedTypeDefinitions()
    {
        return NamedTypeDefinition.QueryAll();
    }

    public IEnumerable<TypeOperator> QueryTypeOperators(string name)
    {
        return TypeOperator.Query(name);
    }

    public IEnumerable<LuaMethodType> QueryTypeOverloads(string name)
    {
        return TypeOverloads.Query(name);
    }

    public IEnumerable<LuaReference> QueryLocalReferences(LuaDeclaration declaration)
    {
        var list = InFiledReferences.Query(declaration.Info.Ptr.DocumentId, declaration.UniqueId);
        if (list is not null)
        {
            return list;
        }

        return [];
    }

    public LuaDeclaration? QueryLocalDeclaration(LuaSyntaxElement element)
    {
        return InFiledDeclarations.Query(element.DocumentId, element.UniqueId);
    }

    public IEnumerable<LuaDeclaration> QueryDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        var tree = DocumentDeclarationTrees.GetValueOrDefault(documentId);
        return tree is not null ? tree.Root.Descendants : [];
    }

    public LuaDeclarationTree? QueryDeclarationTree(LuaDocumentId documentId)
    {
        return DocumentDeclarationTrees.GetValueOrDefault(documentId);
    }
}
