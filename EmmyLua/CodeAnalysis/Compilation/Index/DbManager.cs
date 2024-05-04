using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class DbManager(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private IndexStorage<string, LuaDeclaration> MembersStorage { get; } = new();

    private IndexStorage<long, string> ParentTypesStorage { get; } = new();

    private IndexStorage<string, LuaDeclaration> GlobalsStorage { get; } = new();

    private IndexStorage<string, LuaType> SupersStorage { get; } = new();

    private IndexStorage<string, string> SubTypesStorage { get; } = new();

    private IndexStorage<string, LuaDeclaration> NamedTypeStorage { get; } = new();

    private IndexStorage<long, LuaType> Id2TypeStorage { get; } = new();

    private IndexStorage<string, LuaType> AliasTypesStorage { get; } = new();

    private IndexStorage<string, LuaDeclaration> GenericParamStorage { get; } = new();

    private Dictionary<LuaDocumentId, LuaType> ExportTypesDict { get; } = new();

    private Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ExportExprsDict { get; } = new();

    private TypeOperatorStorage TypeOperatorStorage { get; } = new();

    private IndexStorage<string, LuaElementPtr<LuaNameExprSyntax>> NameExprsStorage { get; } = new();

    private IndexStorage<string, LuaElementPtr<LuaIndexExprSyntax>> IndexExprsStorage { get; } = new();

    private IndexStorage<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameTypesStorage { get; } = new();

    public void Remove(LuaDocumentId documentId)
    {
        MembersStorage.Remove(documentId);
        ParentTypesStorage.Remove(documentId);
        GlobalsStorage.Remove(documentId);
        SupersStorage.Remove(documentId);
        SubTypesStorage.Remove(documentId);
        NamedTypeStorage.Remove(documentId);
        Id2TypeStorage.Remove(documentId);
        GenericParamStorage.Remove(documentId);
        ExportTypesDict.Remove(documentId);
        TypeOperatorStorage.Remove(documentId);
        NameExprsStorage.Remove(documentId);
        IndexExprsStorage.Remove(documentId);
        NameTypesStorage.Remove(documentId);
        AliasTypesStorage.Remove(documentId);
    }

    public void AddMember(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        if (name == "global")
        {
            AddGlobal(documentId, luaDeclaration.Name, luaDeclaration);
            return;
        }

        MembersStorage.Add(documentId, name, luaDeclaration);
        ParentTypesStorage.Add(documentId, luaDeclaration.Info.Ptr.UniqueId, name);
    }

    public void AddGlobal(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GlobalsStorage.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType type)
    {
        SupersStorage.Add(documentId, name, type);
        if (type is LuaNamedType namedType)
        {
            SubTypesStorage.Add(documentId, namedType.Name, name);
        }
    }

    public void AddType(LuaDocumentId documentId, string name, LuaDeclaration declaration)
    {
        NamedTypeStorage.Add(documentId, name, declaration);
    }

    public void AddAlias(LuaDocumentId documentId, string name, LuaType baseType,
        LuaDeclaration declaration)
    {
        AddType(documentId, name, declaration);
        AliasTypesStorage.Add(documentId, name, baseType);
    }

    public void AddIdRelatedType(LuaDocumentId documentId, long id, LuaType relatedType)
    {
        Id2TypeStorage.Add(documentId, id, relatedType);
    }

    public void AddEnum(LuaDocumentId documentId, string name, LuaType? baseType,
        LuaDeclaration declaration)
    {
        AddType(documentId, name, declaration);
        if (baseType != null)
        {
            SupersStorage.Add(documentId, name, baseType);
        }
    }

    public void AddGenericParam(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GenericParamStorage.Add(documentId, name, luaDeclaration);
    }

    public void AddModuleExport(LuaDocumentId documentId, LuaType type, List<LuaExprSyntax> exprs)
    {
        ExportTypesDict[documentId] = type;
        ExportExprsDict[documentId] = exprs.Select(it => it.ToPtr<LuaExprSyntax>()).ToList();
    }

    public void AddNameExpr(LuaDocumentId documentId, LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { RepresentText: { } name })
        {
            NameExprsStorage.Add(documentId, name, new(nameExpr));
        }
    }

    public void AddIndexExpr(LuaDocumentId documentId, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr is { Name: { } name })
        {
            IndexExprsStorage.Add(documentId, name, new(indexExpr));
        }
    }

    public void AddNameType(LuaDocumentId documentId, LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            NameTypesStorage.Add(documentId, name, new(nameType));
        }
    }

    public void AddTypeOperator(LuaDocumentId documentId, TypeOperator typeOperator)
    {
        TypeOperatorStorage.AddTypeOperator(documentId, typeOperator);
    }

    public IEnumerable<LuaDeclaration> GetMembers(string name)
    {
        if (name == "global")
        {
            return GlobalsStorage.GetAll();
        }

        return MembersStorage.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetAllMembers()
    {
        return MembersStorage.GetAll();
    }

    public IEnumerable<LuaDeclaration> GetGlobal(string name)
    {
        return GlobalsStorage.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetGlobals()
    {
        return GlobalsStorage.GetAll();
    }

    public IEnumerable<LuaType> GetSupers(string name)
    {
        return SupersStorage.Get<LuaType>(name);
    }

    public IEnumerable<string> GetSubTypes(string name)
    {
        return SubTypesStorage.Get<string>(name);
    }

    public IEnumerable<LuaDeclaration> GetNamedType(string name)
    {
        return NamedTypeStorage.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaType> GetTypeFromId(long id)
    {
        return Id2TypeStorage.Get<LuaType>(id);
    }

    public IEnumerable<LuaType> GetAliasOriginType(string name)
    {
        return AliasTypesStorage.Get<LuaType>(name);
    }

    public LuaDeclaration? GetTypeLuaDeclaration(string name)
    {
        return NamedTypeStorage.GetOne(name);
    }

    public LuaType? GetModuleExportType(LuaDocumentId documentId)
    {
        return ExportTypesDict.GetValueOrDefault(documentId);
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> GetModuleExportExprs(LuaDocumentId documentId)
    {
        return ExportExprsDict.GetValueOrDefault(documentId) ?? Enumerable.Empty<LuaElementPtr<LuaExprSyntax>>();
    }

    public IEnumerable<LuaDeclaration> GetGenericParams(string name)
    {
        return GenericParamStorage.Get<LuaDeclaration>(name);
    }

    public BasicDetailType GetDetailNamedType(string name, SearchContext context)
    {
        var kind = (NamedTypeStorage.GetOne(name)?.Info as NamedTypeInfo)?.Kind;
        switch (kind)
        {
            case NamedTypeKind.Alias:
            {
                return new AliasDetailType(name, context);
            }
            case NamedTypeKind.Class:
            {
                return new ClassDetailType(name, context);
            }
            case NamedTypeKind.Enum:
            {
                return new EnumDetailType(name, context);
            }
            case NamedTypeKind.Interface:
            {
                return new InterfaceDetailType(name, context);
            }
        }

        return new ClassDetailType(name, context);
    }

    public IEnumerable<LuaNameExprSyntax> GetNameExprs(string name)
    {
        foreach (var nameExprPtr in NameExprsStorage.Get(name))
        {
            if (nameExprPtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaIndexExprSyntax> GetIndexExprs(string name)
    {
        foreach (var indexExprPtr in IndexExprsStorage.Get(name))
        {
            if (indexExprPtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaDocNameTypeSyntax> GetNameTypes(string name)
    {
        foreach (var nameTypePtr in NameTypesStorage.Get(name))
        {
            if (nameTypePtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public bool IsDefinedType(string name)
    {
        return NamedTypeStorage.ContainsKey(name);
    }

    public LuaNamedType? GetParentType(LuaSyntaxNode node)
    {
        var ptr = new LuaElementPtr<LuaSyntaxNode>(node);
        var parentType = ParentTypesStorage.GetLastOne(ptr.UniqueId);
        if (parentType is not null)
        {
            return new LuaNamedType(parentType);
        }

        return null;
    }

    public IEnumerable<LuaDeclaration> GetNamedTypes()
    {
        return NamedTypeStorage.GetAll();
    }

    public IEnumerable<TypeOperator> GetTypeOperators(string typeName)
    {
        return TypeOperatorStorage.GetTypeOperators(typeName);
    }
}
