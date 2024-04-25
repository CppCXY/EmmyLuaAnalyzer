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

    private IndexStorage<string, LuaDeclaration> Members { get; } = new();

    private IndexStorage<long, string> ParentTypes { get; } = new();

    private IndexStorage<string, LuaDeclaration> GlobalDeclaration { get; } = new();

    private IndexStorage<string, LuaType> Supers { get; } = new();

    private IndexStorage<string, string> SubTypes { get; } = new();

    private IndexStorage<string, NamedTypeLuaDeclaration> NamedType { get; } = new();

    private IndexStorage<long, LuaType> Id2Type { get; } = new();

    private IndexStorage<string, LuaType> AliasType { get; } = new();

    private IndexStorage<string, GenericParameterLuaDeclaration> GenericParam { get; } = new();

    private Dictionary<LuaDocumentId, LuaType> ExportTypes { get; } = new();

    private Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ExportExprs { get; } = new();

    public TypeOperatorStorage TypeOperatorStorage { get; } = new();

    private IndexStorage<string, NamedTypeKind> NamedTypeKinds { get; } = new();

    private IndexStorage<string, LuaElementPtr<LuaNameExprSyntax>> NameExprs { get; } = new();

    private IndexStorage<string, LuaElementPtr<LuaIndexExprSyntax>> IndexExprs { get; } = new();

    private IndexStorage<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameTypes { get; } = new();

    public void Remove(LuaDocumentId documentId)
    {
        Members.Remove(documentId);
        ParentTypes.Remove(documentId);
        GlobalDeclaration.Remove(documentId);
        Supers.Remove(documentId);
        SubTypes.Remove(documentId);
        NamedType.Remove(documentId);
        Id2Type.Remove(documentId);
        GenericParam.Remove(documentId);
        ExportTypes.Remove(documentId);
        TypeOperatorStorage.Remove(documentId);
        NamedTypeKinds.Remove(documentId);
        NameExprs.Remove(documentId);
        IndexExprs.Remove(documentId);
        NameTypes.Remove(documentId);
        AliasType.Remove(documentId);
    }

    public void AddMember(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        if (name == "global")
        {
            AddGlobal(documentId, luaDeclaration.Name, luaDeclaration);
            return;
        }

        Members.Add(documentId, name, luaDeclaration);
        ParentTypes.Add(documentId, luaDeclaration.Ptr.UniqueId, name);
    }

    public void AddGlobal(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GlobalDeclaration.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType type)
    {
        Supers.Add(documentId, name, type);
        if (type is LuaNamedType namedType)
        {
            SubTypes.Add(documentId, namedType.Name, name);
        }
    }

    public void AddType(LuaDocumentId documentId, string name, NamedTypeLuaDeclaration luaDeclaration,
        NamedTypeKind kind)
    {
        NamedType.Add(documentId, name, luaDeclaration);
        NamedTypeKinds.Add(documentId, name, kind);
    }

    public void AddAlias(LuaDocumentId documentId, string name, LuaType baseType,
        NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, NamedTypeKind.Alias);
        AliasType.Add(documentId, name, baseType);
    }

    public void AddIdRelatedType(LuaDocumentId documentId, long id, LuaType relatedType)
    {
        Id2Type.Add(documentId, id, relatedType);
    }

    public void AddEnum(LuaDocumentId documentId, string name, LuaType? baseType,
        NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, NamedTypeKind.Enum);
        if (baseType != null)
        {
            Supers.Add(documentId, name, baseType);
        }
    }

    public void AddGenericParam(LuaDocumentId documentId, string name, GenericParameterLuaDeclaration luaDeclaration)
    {
        GenericParam.Add(documentId, name, luaDeclaration);
    }

    public void AddModuleExport(LuaDocumentId documentId, LuaType type, List<LuaExprSyntax> exprs)
    {
        ExportTypes[documentId] = type;
        ExportExprs[documentId] = exprs.Select(it => it.ToPtr<LuaExprSyntax>()).ToList();
    }

    public void AddNameExpr(LuaDocumentId documentId, LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { RepresentText: { } name })
        {
            NameExprs.Add(documentId, name, new(nameExpr));
        }
    }

    public void AddIndexExpr(LuaDocumentId documentId, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr is { Name: { } name })
        {
            IndexExprs.Add(documentId, name, new(indexExpr));
        }
    }

    public void AddNameType(LuaDocumentId documentId, LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            NameTypes.Add(documentId, name, new(nameType));
        }
    }

    public IEnumerable<LuaDeclaration> GetMembers(string name)
    {
        if (name == "global")
        {
            return GlobalDeclaration.GetAll();
        }

        return Members.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetAllMembers()
    {
        return Members.GetAll();
    }

    public IEnumerable<LuaDeclaration> GetGlobal(string name)
    {
        return GlobalDeclaration.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetGlobals()
    {
        return GlobalDeclaration.GetAll();
    }

    public IEnumerable<LuaType> GetSupers(string name)
    {
        return Supers.Get<LuaType>(name);
    }

    public IEnumerable<string> GetSubTypes(string name)
    {
        return SubTypes.Get<string>(name);
    }

    public IEnumerable<NamedTypeLuaDeclaration> GetNamedType(string name)
    {
        return NamedType.Get<NamedTypeLuaDeclaration>(name);
    }

    public IEnumerable<LuaType> GetTypeFromId(long id)
    {
        return Id2Type.Get<LuaType>(id);
    }

    public IEnumerable<LuaType> GetAliasOriginType(string name)
    {
        return AliasType.Get<LuaType>(name);
    }

    public NamedTypeLuaDeclaration? GetTypeLuaDeclaration(string name)
    {
        return NamedType.GetOne(name);
    }

    public LuaType? GetModuleExportType(LuaDocumentId documentId)
    {
        return ExportTypes.GetValueOrDefault(documentId);
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> GetModuleExportExprs(LuaDocumentId documentId)
    {
        return ExportExprs.GetValueOrDefault(documentId) ?? Enumerable.Empty<LuaElementPtr<LuaExprSyntax>>();
    }

    public IEnumerable<GenericParameterLuaDeclaration> GetGenericParams(string name)
    {
        return GenericParam.Get<GenericParameterLuaDeclaration>(name);
    }

    public BasicDetailType GetDetailNamedType(string name, SearchContext context)
    {
        var kind = NamedTypeKinds.GetOne(name);
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
        foreach (var nameExprPtr in NameExprs.Get(name))
        {
            if (nameExprPtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }


    public IEnumerable<LuaIndexExprSyntax> GetIndexExprs(string name)
    {
        foreach (var indexExprPtr in IndexExprs.Get(name))
        {
            if (indexExprPtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaDocNameTypeSyntax> GetNameTypes(string name)
    {
        foreach (var nameTypePtr in NameTypes.Get(name))
        {
            if (nameTypePtr.ToNode(Compilation.Workspace) is { } node)
            {
                yield return node;
            }
        }
    }

    public bool IsDefinedType(string name)
    {
        return NamedTypeKinds.ContainsKey(name);
    }

    public LuaNamedType? GetParentType(LuaSyntaxNode node)
    {
        var ptr = new LuaElementPtr<LuaSyntaxNode>(node);
        var parentType = ParentTypes.GetLastOne(ptr.UniqueId);
        if (parentType is not null)
        {
            return new LuaNamedType(parentType);
        }

        return null;
    }

    public IEnumerable<NamedTypeLuaDeclaration> GetNamedTypes()
    {
        return NamedType.GetAll();
    }
}
