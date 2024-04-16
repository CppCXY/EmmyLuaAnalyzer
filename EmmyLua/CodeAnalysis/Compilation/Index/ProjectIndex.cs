using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class ProjectIndex(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private IndexStorage<string, LuaDeclaration> Members { get; } = new();

    private IndexStorage<string, string> ParentTypes { get; } = new();

    private IndexStorage<string, LuaDeclaration> GlobalDeclaration { get; } = new();

    private IndexStorage<string, LuaType> Supers { get; } = new();

    private IndexStorage<string, NamedTypeLuaDeclaration> NamedType { get; } = new();

    private IndexStorage<string, LuaType> Id2Type { get; } = new();

    private IndexStorage<string, GenericParameterLuaDeclaration> GenericParam { get; } = new();

    private Dictionary<LuaDocumentId, LuaType> ExportTypes { get; } = new();

    public TypeOperatorStorage TypeOperatorStorage { get; } = new();

    private IndexStorage<string, NamedTypeKind> NamedTypeKinds { get; } = new();

    private IndexStorage<string, LuaSyntaxNodePtr<LuaNameExprSyntax>> NameExprs { get; } = new();

    private IndexStorage<string, LuaSyntaxNodePtr<LuaIndexExprSyntax>> IndexExprs { get; } = new();

    private IndexStorage<string, LuaSyntaxNodePtr<LuaDocNameTypeSyntax>> NameTypes { get; } = new();

    public void Remove(LuaDocumentId documentId)
    {
        Members.Remove(documentId);
        ParentTypes.Remove(documentId);
        GlobalDeclaration.Remove(documentId);
        Supers.Remove(documentId);
        NamedType.Remove(documentId);
        Id2Type.Remove(documentId);
        GenericParam.Remove(documentId);
        ExportTypes.Remove(documentId);
        TypeOperatorStorage.Remove(documentId);
        NamedTypeKinds.Remove(documentId);
        NameExprs.Remove(documentId);
        IndexExprs.Remove(documentId);
        NameTypes.Remove(documentId);
    }

    public void AddMember(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        if (name == "global")
        {
            AddGlobal(documentId, name, luaDeclaration);
            return;
        }

        Members.Add(documentId, name, luaDeclaration);
        ParentTypes.Add(documentId, luaDeclaration.Ptr.Stringify, name);
    }

    public void AddGlobal(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GlobalDeclaration.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType type)
    {
        Supers.Add(documentId, name, type);
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
        Id2Type.Add(documentId, name, baseType);
    }

    public void AddRelatedType(LuaDocumentId documentId, string name, LuaType relatedType)
    {
        Id2Type.Add(documentId, name, relatedType);
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

    public void AddMethod(LuaDocumentId documentId, string id, LuaMethodType methodType)
    {
        Id2Type.Add(documentId, id, methodType);
    }

    public void AddGenericParam(LuaDocumentId documentId, string name, GenericParameterLuaDeclaration luaDeclaration)
    {
        GenericParam.Add(documentId, name, luaDeclaration);
    }

    public void AddExportType(LuaDocumentId documentId, LuaType type)
    {
        ExportTypes[documentId] = type;
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
        return Members.Get<LuaDeclaration>(name);
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

    public IEnumerable<NamedTypeLuaDeclaration> GetNamedType(string name)
    {
        return NamedType.Get<NamedTypeLuaDeclaration>(name);
    }

    public IEnumerable<LuaType> GetTypeFromId(string name)
    {
        return Id2Type.Get<LuaType>(name);
    }

    public NamedTypeLuaDeclaration? GetTypeLuaDeclaration(string name)
    {
        return NamedType.GetOne(name);
    }

    public LuaType? GetExportType(LuaDocumentId documentId)
    {
        return ExportTypes.GetValueOrDefault(documentId);
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
        var ptr = new LuaSyntaxNodePtr<LuaSyntaxNode>(node);
        var parentType = ParentTypes.GetLastOne(ptr.Stringify);
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
