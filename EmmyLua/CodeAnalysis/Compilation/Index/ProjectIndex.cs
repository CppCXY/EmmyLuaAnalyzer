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

    private IndexStorage<string, LuaDeclaration> GlobalDeclaration { get; } = new();

    private IndexStorage<string, LuaType> Supers { get; } = new();

    private IndexStorage<string, NamedTypeLuaDeclaration> NamedType { get; } = new();

    private IndexStorage<string, LuaType> Id2Type { get; } = new();

    private IndexStorage<string, GenericParameterLuaDeclaration> GenericParam { get; } = new();

    private Dictionary<DocumentId, LuaType> ExportTypes { get; } = new();

    public TypeOperatorStorage TypeOperatorStorage { get; } = new();

    private IndexStorage<string, NamedTypeKind> NamedTypeKinds { get; } = new();

    private IndexStorage<string, LuaNameExprSyntax> NameExprs { get; } = new();

    private IndexStorage<string, LuaIndexExprSyntax> IndexExprs { get; } = new();

    private IndexStorage<string, LuaDocNameTypeSyntax> NameTypes { get; } = new();

    public void Remove(DocumentId documentId)
    {
        Members.Remove(documentId);
        GlobalDeclaration.Remove(documentId);
        Supers.Remove(documentId);
        NamedType.Remove(documentId);
        GenericParam.Remove(documentId);
        TypeOperatorStorage.Remove(documentId);
        ExportTypes.Remove(documentId);
        Id2Type.Remove(documentId);
        NamedTypeKinds.Remove(documentId);
        NameExprs.Remove(documentId);
        IndexExprs.Remove(documentId);
        NameTypes.Remove(documentId);
    }

    public void AddMember(DocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        Members.Add(documentId, name, luaDeclaration);
    }

    public void AddGlobal(DocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GlobalDeclaration.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(DocumentId documentId, string name, LuaType type)
    {
        Supers.Add(documentId, name, type);
    }

    public void AddType(DocumentId documentId, string name, NamedTypeLuaDeclaration luaDeclaration, NamedTypeKind kind)
    {
        NamedType.Add(documentId, name, luaDeclaration);
        NamedTypeKinds.Add(documentId, name, kind);
    }

    public void AddAlias(DocumentId documentId, string name, LuaType baseType, NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, NamedTypeKind.Alias);
        Id2Type.Add(documentId, name, baseType);
    }

    public void AddRelatedType(DocumentId documentId, string name, LuaType relatedType)
    {
        Id2Type.Add(documentId, name, relatedType);
    }

    public void AddEnum(DocumentId documentId, string name, LuaType? baseType, NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, NamedTypeKind.Enum);
        if (baseType != null)
        {
            Supers.Add(documentId, name, baseType);
        }
    }

    public void AddMethod(DocumentId documentId, string id, LuaMethodType methodType)
    {
        Id2Type.Add(documentId, id, methodType);
    }

    public void AddGenericParam(DocumentId documentId, string name, GenericParameterLuaDeclaration luaDeclaration)
    {
        GenericParam.Add(documentId, name, luaDeclaration);
    }

    public void AddExportType(DocumentId documentId, LuaType type)
    {
        ExportTypes[documentId] = type;
    }

    public void AddNameExpr(DocumentId documentId, LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { RepresentText: { } name })
        {
            NameExprs.Add(documentId, name, nameExpr);
        }
    }

    public void AddIndexExpr(DocumentId documentId, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr is { Name: { } name })
        {
            IndexExprs.Add(documentId, name, indexExpr);
        }
    }

    public void AddNameType(DocumentId documentId, LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            NameTypes.Add(documentId, name, nameType);
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

    public LuaType? GetExportType(DocumentId documentId)
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

    public IEnumerable<LuaNameExprSyntax> GetNameExprs(string name) => NameExprs.Get<LuaNameExprSyntax>(name);

    public IEnumerable<LuaIndexExprSyntax> GetIndexExprs(string name) => IndexExprs.Get<LuaIndexExprSyntax>(name);

    public IEnumerable<LuaDocNameTypeSyntax> GetNameTypes(string name) => NameTypes.Get<LuaDocNameTypeSyntax>(name);

    public bool IsDefinedType(string name)
    {
        return NamedTypeKinds.ContainsKey(name);
    }
}
